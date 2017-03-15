using System.Data.Entity;

namespace LockingWebApp.Locks.Db.Entities.Base
{
    public interface IConfigurationModule
    {
        void Register(DbModelBuilder modelBuilder);
    }
}