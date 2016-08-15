using System;

using Autofac;
using NLog;

namespace Ois.TaskQueues.Worker
{
    public sealed class LoggingModule : Module
    {
        #region Constructors

        public LoggingModule() : base() { }
        #endregion

        #region Module methods overriding

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(GetLogger).As<ILoggerBase, ILogger, Logger>().InstancePerDependency();
        }
        #endregion

        #region Private class methods

        private static Logger GetLogger(IComponentContext context)
        {
            return LogManager.GetCurrentClassLogger();
        }
        #endregion
    }
}