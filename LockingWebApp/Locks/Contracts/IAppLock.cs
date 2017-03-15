using System;
using LockingWebApp.Locks.Dto;

namespace LockingWebApp.Locks.Contracts
{
    public interface IAppLock
    {
        LockAcquisitionResult TryAcquire(string lockName, TimeSpan timeout = default(TimeSpan));
        LockReleaseResult ReleaseLock(string lockName, string lockOwner);
        bool VerifyLockOwnership(string lockName, string lockOwner);
    }
    public interface IEncryptor
    {
        string Encrypt(string val);
        string Decrypt(string val);
    }
}