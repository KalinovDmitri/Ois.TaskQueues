using System;

using Autofac;

namespace Ois.TaskQueues.Worker
{
    using Modules;

    public static class TaskQueueWorkerContainerFactory
    {
        #region Public class methods

        public static IContainer CreateContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule());
            builder.RegisterModule(new ConfigurationModule());
            builder.RegisterModule(new InfrastructureModule());

            return builder.Build();
        }
        #endregion
    }
}