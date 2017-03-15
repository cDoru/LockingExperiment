using System.Data.Entity;
using LockingWebApp.Locks.Contracts;
using LockingWebApp.Locks.Db.Contracts;
using LockingWebApp.Locks.Db.Entities;
using LockingWebApp.Locks.Db.Entities.Base;
using LockingWebApp.Locks.Db.Utils;

namespace LockingWebApp.Locks.Db
{
    public class LockContext : DbContextBase, ILocksContext
    {
        public LockContext(IConfigurationProvider configuration)
            : base(configuration, new LockContextConfigurationModule())
        {
            this.DisableDatabaseInitialization();
        }

        // only used in development
        internal LockContext(string connectionString)
            : base(connectionString, new LockContextConfigurationModule())
        {
            this.DisableDatabaseInitialization();
        }


        public IDbSet<ApplicationLock> Locks { get; set; } 
    }
}