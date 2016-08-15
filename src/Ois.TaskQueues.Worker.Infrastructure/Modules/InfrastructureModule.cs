using System;

using Autofac;
using NLog;

namespace Ois.TaskQueues.Worker.Modules
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

            builder.Register(CreateImplementationService).SingleInstance();
        }
        #endregion

        #region Private class methods

        private TaskQueueWorkerImplementationService CreateImplementationService(IComponentContext context)
        {
            ILogger logger = context.Resolve<ILogger>();

            TaskQueueWorkerImplementationService service = new TaskQueueWorkerImplementationService(logger);

            service.SatisfyImplementors();

            return service;
        }
        #endregion
    }
}