using System;

using Autofac;

namespace Ois.TaskQueues.Service.Modules
{
    using Infrastructure;

    internal sealed class TaskQueueMockModule : Module
    {
        #region Constructors

        public TaskQueueMockModule() : base() { }
        #endregion

        #region Module methods overriding

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TaskQueueClientMock>().As<ITaskQueueClient, TaskQueueClientMock>().InstancePerDependency();
        }
        #endregion
    }
}