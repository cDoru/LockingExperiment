using Autofac;
using LockingWebApp.Locks.Db.Contracts;

namespace LockingWebApp.Locks.Db
{
    public class DalModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LockContext>().As<ILocksContext>().InstancePerLifetimeScope();
        }
    }
}