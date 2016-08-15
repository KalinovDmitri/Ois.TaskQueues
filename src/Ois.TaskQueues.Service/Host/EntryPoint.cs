using System;

using Autofac;
using NLog;
using Topshelf;
using Topshelf.Autofac;
using Topshelf.HostConfigurators;
using Topshelf.ServiceConfigurators;

namespace Ois.TaskQueues.Service
{
    using Modules;

    internal sealed class EntryPoint
    {
        #region Constants and fields

        private static readonly ILogger Logger;
        #endregion

        #region Constructors

        static EntryPoint()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }
        #endregion

        #region The entry point

        internal static int Main(string[] args)
        {
            int result = 0;

            try
            {
                TopshelfExitCode exitCode = HostFactory.Run(ConfigureHost);

                result = (int)exitCode;
            }
            catch (Exception exc)
            {
                result = exc.HResult;
                Logger.Error(exc, exc.Message);
            }

            return result;
        }
        #endregion

        #region Private class methods

        private static void ConfigureHost(HostConfigurator configurator)
        {
            configurator.UseNLog();
            configurator.SetServiceName("TaskQueueService");
            configurator.SetDescription("OIS TaskQueue service");

            ILifetimeScope currentScope = CreateLifetimeScope();
            currentScope.Resolve<TaskQueueServiceConfiguration>();
            configurator.UseAutofacContainer(currentScope);
            configurator.ApplyCommandLine();

            configurator.Service<HostService>(ConfigureHostService);

            configurator.RunAsLocalSystem();
        }

        private static ILifetimeScope CreateLifetimeScope()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterModule(new MainModule());

            return builder.Build();
        }

        private static void ConfigureHostService(ServiceConfigurator<HostService> configurator)
        {
            configurator.ConstructUsingAutofacContainer();

            configurator.WhenStarted(HostServiceStarted);
            configurator.WhenStopped(HostServiceStopped);
        }

        private static bool HostServiceStarted(HostService host, HostControl control)
        {
            return host.Start();
        }

        private static bool HostServiceStopped(HostService host, HostControl control)
        {
            return host.Stop();
        }
        #endregion
    }
}