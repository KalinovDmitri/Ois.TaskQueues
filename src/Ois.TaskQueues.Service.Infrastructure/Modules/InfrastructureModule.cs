﻿using System;

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

            builder.RegisterType<TaskQueueBalancingService>().As<IStartable, TaskQueueBalancingService>().SingleInstance();
            builder.RegisterType<TaskQueueNotificationService>().As<IStartable, TaskQueueNotificationService>().SingleInstance();
            builder.RegisterType<TaskQueueClientService>().SingleInstance();
            builder.RegisterType<TaskQueueQueueService>().SingleInstance();
            builder.RegisterType<TaskQueueWorkerService>().SingleInstance();
            builder.RegisterType<TaskQueueProcessingService>().SingleInstance();

            builder.RegisterType<TaskQueueServiceImplementor>().PropertiesAutowired(PropertyWiringOptions.None).SingleInstance();
        }
        #endregion
    }
}