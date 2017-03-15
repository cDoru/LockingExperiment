using System.Data.Entity.Infrastructure;

namespace LockingWebApp.Locks.Db
{
#if DEBUG
    public class ContextFactory : IDbContextFactory<LockContext>
    {
        public LockContext Create()
        {
            return new LockContext(Settings.DbConnection);
        }
    }
#endif
}