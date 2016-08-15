using System;

using Autofac;

namespace Ois.TaskQueues.Service
{
    using Modules;

    public static class TaskQueueServiceContainerFactory
    {
        #region Public class methods

        public static IContainer CreateContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule());
            builder.RegisterModule(new ConfigurationModule());
            builder.RegisterModule(new InfrastructureModule());

            builder.RegisterModule(new TaskQueueMockModule());

            return builder.Build();
        }
        #endregion
    }
}