using System;

namespace LockingWebApp.Locks.Db.Entities.Base
{
    public interface IGuidEntity
    {
        Guid Id { get; }
    }
}