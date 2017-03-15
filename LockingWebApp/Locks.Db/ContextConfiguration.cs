using System.Data.Entity.Migrations;

namespace LockingWebApp.Locks.Db
{
    public class ContextConfiguration : DbMigrationsConfiguration<LockContext>
    {
        public ContextConfiguration()
        {
            // disable automatic migrations
            AutomaticMigrationsEnabled = false;
        }
    }
}