using System;
using System.ServiceModel;

using Autofac;
using Autofac.Integration.Wcf;
using NLog;

namespace Ois.TaskQueues.Communication
{
    internal sealed class TaskQueueWorkerServiceHostManager : IStartable, IDisposable
    {
        #region Constants and fields

        private const string ServiceAddressFormat = "net.tcp://{0}:{1}/";

        private readonly ILifetimeScope Container;

        private readonly ILogger Logger;

        private readonly TaskQueueServiceConfiguration Configuration;

        private ServiceHost Host = null;
        #endregion

        #region Constructors

        public TaskQueueWorkerServiceHostManager(ILifetimeScope container, ILogger logger, TaskQueueServiceConfiguration configuration)
        {
            Container = container;
            Logger = logger;
            Configuration = configuration;
        }
        #endregion

        #region IStartable implementation

        public void Start()
        {
            if (Host != null) return;

            try
            {
                ServiceHost host = CreateHost();
                host.Open();
                Host = host;

                Logger.Info("The ITaskQueueWorkerService host created successfully");
            }
            catch (Exception exc)
            {
                Logger.Error(exc, exc.Message);
            }
        }
        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (Host != null)
            {
                try
                {
                    Host.Close(); Host = null;

                    Logger.Info("The ITaskQueueWorkerService host stopped successfully");
                }
                catch (Exception exc)
                {
                    Logger.Error(exc, exc.Message);
                }
            }
        }
        #endregion

        #region Private class methods

        private ServiceHost CreateHost()
        {
            string serviceAddress = string.Format(ServiceAddressFormat, Configuration.ServiceHost, Configuration.WorkerServicePort);

            ServiceHost host = new ServiceHost(typeof(TaskQueueWorkerService), new Uri[]
            {
                new Uri(serviceAddress)
            });

            host.AddDependencyInjectionBehavior<ITaskQueueWorkerService>(Container);

            return host;
        }
        #endregion
    }
}