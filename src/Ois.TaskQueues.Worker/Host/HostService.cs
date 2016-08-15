using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

namespace Ois.TaskQueues.Worker
{
    internal sealed class HostService
    {
        #region Constants and fields

        private readonly IDisposable Disposer;

        private readonly IEnumerable<Lazy<IStartable>> Startables;

        private readonly ILogger Logger;
        #endregion

        #region Constructors

        public HostService(IDisposable disposer, IEnumerable<Lazy<IStartable>> startables, ILogger logger)
        {
            Disposer = disposer;
            Startables = startables;
            Logger = logger;
        }
        #endregion

        #region Public class methods

        public bool Start()
        {
            Logger.Debug("Starting HostService...");

            foreach (Lazy<IStartable> item in Startables.Reverse())
            {
                item.Value.Start();
            }

            Logger.Debug("HostService has been started successfully.");
            return true;
        }

        public bool Stop()
        {
            Logger.Debug("Stopping HostService...");

            Disposer.Dispose();

            Logger.Debug("HostService has been stopped successfully.");
            return true;
        }
        #endregion
    }
}