using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NLog;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueWorkerService
    {
        #region Constants and fields

        private readonly ILogger Logger;

        private readonly ConcurrentDictionary<Guid, TaskQueueWorkerEntry> OverallWorkers;

        private readonly ConcurrentDictionary<string, ConcurrentQueue<TaskQueueWorkerEntry>> WorkerQueues;

        private readonly ConcurrentDictionary<Guid, TaskQueueBusyWorkerEntry> BusyWorkers;

        private readonly Func<string, ConcurrentQueue<TaskQueueWorkerEntry>> WorkerQueueCreator;

        private int CountOfWorkers;

        private int CountOfAvailableWorkers;
        #endregion

        #region Properties

        public int WorkersCount => CountOfWorkers;

        public int AvailableWorkersCount => CountOfAvailableWorkers;
        #endregion

        #region Constructors

        private TaskQueueWorkerService()
        {
            OverallWorkers = new ConcurrentDictionary<Guid, TaskQueueWorkerEntry>();
            WorkerQueues = new ConcurrentDictionary<string, ConcurrentQueue<TaskQueueWorkerEntry>>(StringComparer.OrdinalIgnoreCase);
            BusyWorkers = new ConcurrentDictionary<Guid, TaskQueueBusyWorkerEntry>();

            WorkerQueueCreator = new Func<string, ConcurrentQueue<TaskQueueWorkerEntry>>(CreateWorkerQueue);
        }

        public TaskQueueWorkerService(ILogger logger) : this()
        {
            Logger = logger;
        }
        #endregion

        #region Public class methods

        public bool AddWorker(Guid workerID, ITaskQueueWorker worker, string[] taskCategories)
        {
            TaskQueueWorkerEntry workerEntry = new TaskQueueWorkerEntry(workerID, worker);

            bool isAdded = OverallWorkers.TryAdd(workerID, workerEntry);
            if (isAdded)
            {
                int categoriesCount = taskCategories.Length;
                for (int index = 0; index < categoriesCount; ++index)
                {
                    string currentCategory = taskCategories[index];
                    var queue = WorkerQueues.GetOrAdd(currentCategory, WorkerQueueCreator);
                    queue.Enqueue(workerEntry);
                }

                Interlocked.Increment(ref CountOfWorkers);
                Interlocked.Increment(ref CountOfAvailableWorkers);
            }

            return isAdded;
        }

        public void KeepAliveWorker(Guid workerID)
        {
            TaskQueueWorkerEntry workerEntry = null;
            bool isExists = OverallWorkers.TryGetValue(workerID, out workerEntry);
            if (isExists)
            {
                workerEntry.UpdateAliveTime();
            }
        }

        public void ReleaseWorker(Guid workerID)
        {
            TaskQueueBusyWorkerEntry busyWorker = null;
            bool isReleased = BusyWorkers.TryRemove(workerID, out busyWorker);
            if (isReleased)
            {
                busyWorker.Release();
                Interlocked.Increment(ref CountOfAvailableWorkers);
            }
        }

        public bool RemoveWorker(Guid workerID)
        {
            TaskQueueBusyWorkerEntry busyWorker = null;
            bool isRemoved = BusyWorkers.TryRemove(workerID, out busyWorker);
            if (isRemoved)
            {
                busyWorker.Worker.Dispose();
            }

            TaskQueueWorkerEntry workerEntry = null;
            isRemoved = OverallWorkers.TryRemove(workerID, out workerEntry);
            if (isRemoved)
            {
                workerEntry.Dispose();
            }

            if (isRemoved)
            {
                Interlocked.Decrement(ref CountOfAvailableWorkers);
            }

            return isRemoved;
        }
        #endregion

        #region Internal class methods

        internal bool TryDequeueWorker(string taskCategory, out TaskQueueWorkerEntry workerEntry)
        {
            bool result = false;
            var queue = WorkerQueues.GetOrAdd(taskCategory, WorkerQueueCreator);

            while (true)
            {
                result = queue.TryDequeue(out workerEntry);
                if (result)
                {
                    if (BusyWorkers.ContainsKey(workerEntry.WorkerID))
                    {
                        queue.Enqueue(workerEntry); continue;
                    }
                    else if (RemoveWorkerIfDisposed(workerEntry))
                    {
                        continue;
                    }
                    else
                    {
                        var busyEntry = new TaskQueueBusyWorkerEntry(queue, workerEntry);
                        bool isAdded = BusyWorkers.TryAdd(workerEntry.WorkerID, busyEntry);
                        if (isAdded)
                        {
                            Interlocked.Decrement(ref CountOfAvailableWorkers); break;
                        }
                        else
                        {
                            queue.Enqueue(workerEntry); continue;
                        }
                    }
                }
                else
                {
                    workerEntry = null; break;
                }
            }

            return result;
        }

        internal void DisposeWorker(Guid workerID)
        {
            TaskQueueBusyWorkerEntry busyEntry = null;
            bool isRemoved = BusyWorkers.TryRemove(workerID, out busyEntry);
            if (isRemoved)
            {
                busyEntry.Worker.Dispose();
                busyEntry.Release();
                Interlocked.Increment(ref CountOfAvailableWorkers);
            }
        }
        #endregion

        #region Private class methods

        private ConcurrentQueue<TaskQueueWorkerEntry> CreateWorkerQueue(string taskCategory)
        {
            return new ConcurrentQueue<TaskQueueWorkerEntry>();
        }

        private bool RemoveWorkerIfDisposed(TaskQueueWorkerEntry workerEntry)
        {
            bool result = workerEntry.IsDisposed;
            if (result)
            {
                TaskQueueWorkerEntry removedWorker = null;
                bool isRemoved = OverallWorkers.TryRemove(workerEntry.WorkerID, out removedWorker);
                if (isRemoved)
                {
                    Interlocked.Decrement(ref CountOfWorkers);
                    workerEntry.Close();
                }
            }
            return result;
        }
        #endregion
    }
}