using System;

using Autofac;

namespace Ois.TaskQueues.Service.Modules
{
    using Communication;

    internal sealed class CommunicationModule : Module
    {
        #region Constructors

        public CommunicationModule() : base() { }
        #endregion

        #region Module methods overriding

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TaskQueueClientService>().As<ITaskQueueClientService>().InstancePerLifetimeScope();
            builder.RegisterType<TaskQueueClientServiceHostManager>().As<IStartable>().SingleInstance();

            builder.RegisterType<TaskQueueWorkerService>().As<ITaskQueueWorkerService>().InstancePerLifetimeScope();
            builder.RegisterType<TaskQueueWorkerServiceHostManager>().As<IStartable>().SingleInstance();
        }
        #endregion
    }
}