using System.Data.Entity;
using LockingWebApp.Locks.Db.Entities;
using LockingWebApp.Locks.Db.Entities.Base;

namespace LockingWebApp.Locks.Db.Contracts
{
    public interface ILocksContext : IDbContext
    {
        IDbSet<ApplicationLock> Locks { get; } 
    }
}