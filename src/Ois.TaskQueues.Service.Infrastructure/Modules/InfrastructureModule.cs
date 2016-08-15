using System;

using Autofac;

namespace Ois.TaskQueues.Service.Modules
{
    using Infrastructure;

    public sealed class InfrastructureModule : Module
    {
        #region Constructors

        public InfrastructureModule() : base() { }
        #endregion

        #region Module methods overriding

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // TODO: implement infrastructure services registration

            builder.RegisterType<TaskQueueNotificationService>().As<IStartable, TaskQueueNotificationService>().SingleInstance();
            builder.RegisterType<TaskQueueClientService>().PropertiesAutowired(PropertyWiringOptions.None).SingleInstance();
            builder.RegisterType<TaskQueueComputationService>().PropertiesAutowired(PropertyWiringOptions.None).SingleInstance();
            builder.RegisterType<TaskQueueWorkerService>().PropertiesAutowired(PropertyWiringOptions.None).SingleInstance();
            builder.RegisterType<TaskQueueProcessingService>().PropertiesAutowired(PropertyWiringOptions.None).SingleInstance();

            builder.RegisterType<TaskQueueServiceImplementor>().PropertiesAutowired(PropertyWiringOptions.None).SingleInstance();
        }
        #endregion
    }
}