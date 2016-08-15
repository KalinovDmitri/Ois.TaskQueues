using System;

using Autofac;
using NLog;

namespace Ois.TaskQueues.Worker.Modules
{
    public sealed class CommunicationModule : Module
    {
        #region Fields

        private Type[] WorkerConstructorParameterTypes;
        #endregion

        #region Constructors

        public CommunicationModule() : base()
        {
            WorkerConstructorParameterTypes = new [] { typeof(ILogger), typeof(TaskQueueWorkerConfiguration) };
        }
        #endregion

        #region Module methods overriding

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
                .RegisterType<TaskQueueWorker>()
                .As<IStartable, IDisposable, TaskQueueWorker>()
                .UsingConstructor(WorkerConstructorParameterTypes)
                .PropertiesAutowired()
                .SingleInstance();
        }
        #endregion
    }
}