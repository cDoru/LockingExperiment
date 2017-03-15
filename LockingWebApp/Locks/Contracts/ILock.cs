using System;

namespace LockingWebApp.Locks.Contracts
{
    public interface ILock
    {
        IDisposableShim Acquire(string lockName, TimeSpan timeout = default(TimeSpan));
        IDisposableShim AcquireWithFail(string lockName, TimeSpan timeout = default(TimeSpan));
    }
}