using System;
using System.Reflection;

using Autofac;
using Autofac.Core;

namespace Ois.TaskQueues.Worker.Modules
{
    public sealed class MainModule : Autofac.Module
    {
        #region Constructors

        public MainModule() : base() { }
        #endregion

        #region Module methods overriding

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // регистрация модулей инфраструктуры воркера
            builder.RegisterModule(new LoggingModule());
            builder.RegisterModule(new ConfigurationModule());
            builder.RegisterModule(new InfrastructureModule());
            builder.RegisterModule(new CommunicationModule());

            // регистрация хост-сервиса
            ResolvedParameter serviceParameter = new ResolvedParameter(HostServiceParameterResolver, HostServiceParameterAccessor);
            builder.RegisterType<HostService>().WithParameter(serviceParameter).SingleInstance();
        }
        #endregion

        #region Private class methods

        private static bool HostServiceParameterResolver(ParameterInfo info, IComponentContext context)
        {
            return typeof(IDisposable).IsAssignableFrom(info.ParameterType);
        }

        private static object HostServiceParameterAccessor(ParameterInfo info, IComponentContext context)
        {
            return context.Resolve<ILifetimeScope>();
        }
        #endregion
    }
}