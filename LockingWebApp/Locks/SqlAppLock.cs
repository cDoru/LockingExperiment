using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using LockingWebApp.Locks.Configuration;
using LockingWebApp.Locks.Contracts;
using LockingWebApp.Locks.Dto;

namespace LockingWebApp.Locks
{
    public sealed class SqlAppLock : IAppLock
    {
        private readonly IEncryptor _encryptor;
        private readonly string _connectionString;

        public SqlAppLock(IConfigurationProvider configurationProvider, IEncryptor encryptor)
        {
            _encryptor = encryptor;
            _connectionString = configurationProvider.GetConfigurationValue(ConfigurationKeys.DbConnection);
        }

        /// <summary>
        /// The maximum allowed length for lock names. See https://msdn.microsoft.com/en-us/library/ms189823.aspx
        /// </summary>
        public static int MaxLockNameLength
        {
            get { return 255; }
        }

        private static DateTime Utc
        {
            get { return ConvertToUtc(DateTime.Now); }
        }

        private static DateTime ConvertToUtc(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Unspecified:
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                case DateTimeKind.Local:
                    return dateTime.ToUniversalTime();
                default:
                    return dateTime;
            }
        }

        /// <summary>
        /// Attempts to acquire the lock synchronously. Usage:
        /// <code>
        ///     using (var handle = myLock.TryAcquire(...))
        ///     {
        ///         if (handle != null) { /* we have the lock! */ }
        ///     }
        ///     // dispose releases the lock if we took it
        /// </code>
        /// </summary>
        /// <param name="lockName"></param>
        /// <param name="timeout">How long to wait before giving up on acquiring the lock. Defaults to 0</param>
        /// <returns>An <see cref="IDisposable"/> "handle" which can be used to release the lock, or null if the lock was not taken</returns>
        public LockAcquisitionResult TryAcquire(string lockName, TimeSpan timeout = default(TimeSpan))
        {
            // synchronous mode
            var timeoutMillis = timeout.ToInt32Timeout();

            DbConnection acquireConnection = null;
            var cleanup = true;
            try
            {
                acquireConnection = GetConnection();

                if (_connectionString != null)
                {
                    acquireConnection.Open();
                }
                else if (acquireConnection == null)
                {
                    throw new InvalidOperationException("The transaction had been disposed");
                }
                else if (acquireConnection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("The connection is not open");
                }

                var checkCommand = CreateCheckLockAvailabilityCommand(acquireConnection, timeoutMillis, lockName);
                var exists = (int)checkCommand.ExecuteScalar() > 0;
                if (exists) return LockAcquisitionResult.Fail;

                var id = Guid.NewGuid();

                SqlParameter insertReturnValue;
                using (var insertCommand = CreateInsertApplicationLockCommand(acquireConnection, id,
                    timeoutMillis, lockName, Utc, out insertReturnValue))
                {
                    insertCommand.ExecuteNonQuery();
                }

                var ret = (int)insertReturnValue.Value;
                cleanup = ret == 0;
                var success = ret == 0;
                var owner = string.Empty;

                if (success)
                {
                    // hash owner
                    owner = _encryptor.Encrypt(id.ToString());

                    // check no duplicates.
                    var checkDuplicateCommand = CreateCheckLockAvailabilityCommand(acquireConnection, timeoutMillis, lockName);
                    var duplicatesExist = (int)checkDuplicateCommand.ExecuteScalar() > 1;

                    if (duplicatesExist)
                    {
                        // delete current lock
                        ReleaseLock(lockName, owner);
                        return LockAcquisitionResult.Fail;
                    }
                }

                return new LockAcquisitionResult
                {
                    Success = success,
                    LockOwner = owner
                };

            }
            catch
            {
                // in case we fail to create lock scope or something
                cleanup = true;
                throw;
            }
            finally
            {
                if (cleanup)
                {
                    Cleanup(acquireConnection);
                }
            }
        }

        private void Cleanup(IDisposable connection)
        {
            // dispose connection and transaction unless they are externally owned
            if (_connectionString == null) return;
            if (connection != null)
            {
                connection.Dispose();
            }
        }

        public LockReleaseResult ReleaseLock(string lockName, string lockOwner)
        {
            // otherwise issue the release command
            var connection = GetConnection();
            if (_connectionString != null)
            {
                connection.Open();
            }
            else if (connection == null)
            {
                throw new InvalidOperationException("The transaction had been disposed");
            }
            else if (connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open");
            }

            var id = Guid.Parse(_encryptor.Decrypt(lockOwner));
            using (var checkCommand = CreateCheckApplicationLockCommand(connection, 1000, lockName, id))
            {
                var exists = (int)checkCommand.ExecuteScalar() > 0;

                if (!exists)
                {
                    return new LockReleaseResult
                    {
                        Success = false,
                        Reason = ReleaseLockFailure.OwnerNotMatching
                    };
                }

                SqlParameter deleteReturnValue;

                using (
                    var releaseCommand = CreateDeleteApplicationLockCommand(connection,
                        1000, lockName, id, out deleteReturnValue))
                {
                    releaseCommand.ExecuteNonQuery();
                }

                var success = (int)deleteReturnValue.Value == 0;

                return success
                    ? new LockReleaseResult
                    {
                        Success = true,
                        Reason = ReleaseLockFailure.Undefined
                    }
                    : new LockReleaseResult
                    {
                        Success = false,
                        Reason = ReleaseLockFailure.ReleaseError
                    };
            }
        }

        public bool VerifyLockOwnership(string lockName, string lockOwner)
        {
            // otherwise issue the release command
            var connection = GetConnection();
            if (_connectionString != null)
            {
                connection.Open();
            }
            else if (connection == null)
            {
                throw new InvalidOperationException("The transaction had been disposed");
            }
            else if (connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection is not open");
            }

            var id = Guid.Parse(_encryptor.Decrypt(lockOwner));
            using (var checkCommand = CreateCheckApplicationLockCommand(connection, 1000, lockName, id))
            {
                var exists = (int)checkCommand.ExecuteScalar() > 0;
                return exists;
            }
        }

        private DbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private static DbCommand CreateCheckApplicationLockCommand(DbConnection connection, int timeoutMillis, string lockName,
            Guid lockOwner)
        {
            var command = connection.CreateCommand();
            //command.Transaction = transaction;
            command.CommandText = "select COUNT(*) from [dbo].[ApplicationLock] where [LockName] = @lockName and [Id] = @owner";

            command.CommandType = CommandType.Text;
            command.CommandTimeout = timeoutMillis >= 0
                // command timeout is in seconds. We always wait at least the lock timeout plus a buffer 
                ? (timeoutMillis / 1000) + 30
                // otherwise timeout is infinite so we use the infinite timeout of 0
                // (see https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.commandtimeout%28v=vs.110%29.aspx)
                : 0;

            command.Parameters.Add(CreateStringParameter(command, "lockName", lockName));
            command.Parameters.Add(CreateUniqueidentityParameter(command, "owner", lockOwner));
            return command;
        }


        private static DbCommand CreateDeleteApplicationLockCommand(DbConnection connection,
            int timeoutMillis, string lockName, Guid lockOwner, out SqlParameter returnValue)
        {

            var command = connection.CreateCommand();
            //command.Transaction = transaction;
            command.CommandText = "delete from [dbo].[ApplicationLock] where [LockName] = @lockName and [Id] = @owner";

            command.CommandType = CommandType.Text;
            command.CommandTimeout = timeoutMillis >= 0
                // command timeout is in seconds. We always wait at least the lock timeout plus a buffer 
                ? (timeoutMillis / 1000) + 30
                // otherwise timeout is infinite so we use the infinite timeout of 0
                // (see https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.commandtimeout%28v=vs.110%29.aspx)
                : 0;

            command.Parameters.Add(CreateStringParameter(command, "lockName", lockName));
            command.Parameters.Add(CreateUniqueidentityParameter(command, "owner", lockOwner));
            command.Parameters.Add(returnValue = new SqlParameter { Direction = ParameterDirection.ReturnValue });
            return command;
        }

        private static DbCommand CreateCheckLockAvailabilityCommand(DbConnection connection, int timeoutMillis,
            string lockName)
        {
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "select count(*) from [dbo].[ApplicationLock] where [LockName] = @lockName";
            checkCommand.CommandType = CommandType.Text;
            checkCommand.CommandTimeout = timeoutMillis >= 0
                // command timeout is in seconds. We always wait at least the lock timeout plus a buffer 
                ? (timeoutMillis / 1000) + 30
                // otherwise timeout is infinite so we use the infinite timeout of 0
                // (see https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.commandtimeout%28v=vs.110%29.aspx)
                : 0;
            checkCommand.Parameters.Add(CreateStringParameter(checkCommand, "@lockName", lockName));
            return checkCommand;
        }


        private static DbCommand CreateInsertApplicationLockCommand(DbConnection connection, Guid id, int timeoutMillis,
            string lockName, DateTime utcTimestamp, out SqlParameter returnValue)
        {

            var insertCommand = connection.CreateCommand();
            //command.Transaction = transaction;
            insertCommand.CommandText = @"INSERT INTO [dbo].[ApplicationLock]
                                       ([Id]
                                       ,[UtcTimestamp]
                                       ,[LockName])
                                        VALUES
                                       (@Id
                                       ,@UtcTimestamp
                                       ,@LockName)";

            insertCommand.CommandType = CommandType.Text;
            insertCommand.CommandTimeout = timeoutMillis >= 0
                // command timeout is in seconds. We always wait at least the lock timeout plus a buffer 
                ? (timeoutMillis / 1000) + 30
                // otherwise timeout is infinite so we use the infinite timeout of 0
                // (see https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.commandtimeout%28v=vs.110%29.aspx)
                : 0;

            insertCommand.Parameters.Add(CreateUniqueidentityParameter(insertCommand, "Id", id));
            insertCommand.Parameters.Add(CreateDateParameter(insertCommand, "UtcTimestamp", utcTimestamp));
            insertCommand.Parameters.Add(CreateStringParameter(insertCommand, "LockName", lockName));

            insertCommand.Parameters.Add(returnValue = new SqlParameter { Direction = ParameterDirection.ReturnValue });
            return insertCommand;
        }

        private static DbParameter CreateParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }

        private static DbParameter CreateStringParameter(DbCommand cmd, string name, string value)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.Direction = ParameterDirection.Input;
            parameter.DbType = DbType.String;
            parameter.Value = value;

            return parameter;
        }

        private static DbParameter CreateDateParameter(DbCommand cmd, string name, DateTime date)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.DbType = DbType.DateTime;
            param.Direction = ParameterDirection.Input;
            param.Value = date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return param;
        }

        private static DbParameter CreateUniqueidentityParameter(DbCommand cmd, string name, Guid uniqueidentifier)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = DbType.Guid;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = uniqueidentifier;
            return parameter;
        }
    }
}