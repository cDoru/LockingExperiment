using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using LockingWebApp.Locks.Db.Entities.Base;
using LockingWebApp.Locks.Db.Utils;

namespace LockingWebApp.Locks.Db.Entities
{
    public class ApplicationLock : IGuidEntity
    {
        public Guid Id { get; private set; }

        public DateTime UtcTimestamp { get; set; }

        public string LockName { get; set; }

        public ApplicationLock()
        {
            Id = SequentialGuid.NewSequentialGuid();
        }
    }
}