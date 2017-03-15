using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Web;

namespace LockingWebApp.Locks.Utils
{
    public class SqlHelpers
    {
        /// <summary>
        /// Creates simple param
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DbParameter CreateParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }

        /// <summary>
        /// Creates string param
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DbParameter CreateStringParameter(DbCommand cmd, string name, string value)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.Direction = ParameterDirection.Input;
            parameter.DbType = DbType.String;
            parameter.Value = value;

            return parameter;
        }

        /// <summary>
        /// Creates datetime param
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DbParameter CreateDateParameter(DbCommand cmd, string name, DateTime date)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.DbType = DbType.DateTime;
            param.Direction = ParameterDirection.Input;
            param.Value = date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return param;
        }

        /// <summary>
        /// Creates GUID param
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="name"></param>
        /// <param name="uniqueidentifier"></param>
        /// <returns></returns>
        public static DbParameter CreateUniqueidentityParameter(DbCommand cmd, string name, Guid uniqueidentifier)
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