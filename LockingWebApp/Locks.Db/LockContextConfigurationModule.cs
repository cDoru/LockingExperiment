using System.Data.Entity;
using LockingWebApp.Locks.Db.Configurations;
using LockingWebApp.Locks.Db.Entities.Base;

namespace LockingWebApp.Locks.Db
{
    public class LockContextConfigurationModule : IConfigurationModule
    {
        public void Register(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ApplicationLockConfiguration());
        }
    }
}