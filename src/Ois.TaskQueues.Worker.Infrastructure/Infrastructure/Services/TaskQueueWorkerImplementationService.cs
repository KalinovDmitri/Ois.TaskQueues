using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using NLog;

namespace Ois.TaskQueues.Worker.Infrastructure
{
    public sealed class TaskQueueWorkerImplementationService
    {
        #region Constants and fields

        private readonly ILogger Logger;

        private readonly ConcurrentDictionary<string, ITaskQueueWorkerImpl> ImplementorsDictionary;

        private AggregateCatalog CurrentCatalog;

        private CompositionContainer CurrentContainer;

        [ImportMany(typeof(ITaskQueueWorkerImpl), AllowRecomposition = true)]
        internal List<Lazy<ITaskQueueWorkerImpl, ITaskQueueWorkerMetadata>> Implementors { get; private set; }
        #endregion

        #region Constructors

        private TaskQueueWorkerImplementationService()
        {
            ImplementorsDictionary = new ConcurrentDictionary<string, ITaskQueueWorkerImpl>(StringComparer.OrdinalIgnoreCase);

            Implementors = new List<Lazy<ITaskQueueWorkerImpl, ITaskQueueWorkerMetadata>>();

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryCatalog directory = new DirectoryCatalog(currentDirectory);
            AggregateCatalog aggregation = new AggregateCatalog(directory);

            CurrentCatalog = aggregation;
            CurrentContainer = new CompositionContainer(aggregation);
        }

        public TaskQueueWorkerImplementationService(ILogger logger) : this()
        {
            Logger = logger;
        }
        #endregion

        #region Public class methods

        public void SatisfyImplementors()
        {
            try
            {
                CurrentContainer.ComposeParts(new object[] { this });
            }
            catch (ReflectionTypeLoadException loadExc)
            {
                AggregateException aggregateExc = new AggregateException(loadExc.LoaderExceptions);

                Logger.Error(aggregateExc, loadExc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc, exc.Message);
            }
        }

        public ITaskQueueWorkerImpl ResolveImplementor(string taskCategory)
        {
            return ImplementorsDictionary.GetOrAdd(taskCategory, ResolveImplementorCore);
        }
        #endregion

        #region Private class methods

        private ITaskQueueWorkerImpl ResolveImplementorCore(string taskCategory)
        {
            ITaskQueueWorkerImpl target = null;

            var implementors = Implementors;
            if (implementors != null)
            {
                Lazy<ITaskQueueWorkerImpl, ITaskQueueWorkerMetadata> targetEntry = null;

                int count = implementors.Count;
                for (int index = 0; index < count; ++index)
                {
                    targetEntry = implementors[index];

                    if (string.Equals(taskCategory, targetEntry.Metadata.TaskCategory, StringComparison.OrdinalIgnoreCase))
                    {
                        target = targetEntry.Value;
                        break;
                    }
                }
            }

            return target;
        }
        #endregion
    }
}