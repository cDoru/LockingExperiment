namespace LockingWebApp.Locks.Dto
{
    public enum ReleaseLockFailure
    {
        Undefined,
        ReleaseError,
        OwnerNotMatching
    }
}