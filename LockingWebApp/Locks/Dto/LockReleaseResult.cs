namespace LockingWebApp.Locks.Dto
{
    public class LockReleaseResult
    {
        public bool Success { get; set; }
        public ReleaseLockFailure Reason { get; set; }
    }
}