using System.Data.Entity;
using System.Reflection;

namespace LockingWebApp.Locks.Db.Utils
{
    internal static class DbContextExtensions
    {
        public static void DisableDatabaseInitialization(this DbContext dbContext)
        {
            var databaseType = typeof(Database);
            var setInitializer = databaseType.GetMethod("SetInitializer", BindingFlags.Static | BindingFlags.Public);
            var type = dbContext.GetType();
            var setInitializerT = setInitializer.MakeGenericMethod(type);

            setInitializerT.Invoke(null, new object[] { null });
        }
    }
}