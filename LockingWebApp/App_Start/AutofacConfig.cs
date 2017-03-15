using System.Data.Entity;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using LockingWebApp.Locks.Configuration;
using LockingWebApp.Locks.Contracts;
using LockingWebApp.Locks.Db;
using LockingWebApp.Utils;

namespace LockingWebApp.App_Start
{
    public class AutofacConfig
    {
        private class Inner { }
        public static IContainer Container { get; private set; }

        public static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.Register<IConfigurationProvider>(c => new ConfigurationProvider()).SingleInstance();
            builder.RegisterType<Encryptor>().As<IEncryptor>().SingleInstance();

            builder.RegisterModule(new DalModule());
            AutowireProperties(builder);
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            Container = builder.Build();

            DependencyResolver.SetResolver(new AutofacResolver(Container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
            DbConfiguration.Loaded += DbConfiguration_Loaded;
        }

        private static void AutowireProperties(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(typeof(Inner).Assembly)
                .PropertiesAutowired();

            builder.RegisterType<WebApiApplication>()
                .PropertiesAutowired();
        }

        static void DbConfiguration_Loaded(object sender, DbConfigurationLoadedEventArgs e)
        {
            e.AddDependencyResolver(new WrappingEfAutofacResolver(e.DependencyResolver, Container), true);
        }
    }
}