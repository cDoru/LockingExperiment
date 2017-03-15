using System;

namespace LockingWebApp.Locks.Contracts
{
    public interface IDisposableShim : IDisposable
    {
        bool AcquisitionFailed { get; }
    }
}