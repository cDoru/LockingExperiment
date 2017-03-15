namespace LockingWebApp.Locks.Db
{
    internal static class Settings
    {
#if DEBUG
        public const string DbConnection = "Data Source=.;Initial Catalog=MultitenancyExperiment;Integrated Security=True;";
#endif
    }
}