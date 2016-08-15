using System;
using System.Collections.Concurrent;

namespace Ois.TaskQueues.Service.Infrastructure
{
    internal sealed class TaskQueueBusyWorkerEntry
    {
        #region Constants and fields

        private readonly ConcurrentQueue<TaskQueueWorkerEntry> SourceQueue;

        public readonly TaskQueueWorkerEntry Worker;
        #endregion

        #region Constructors

        public TaskQueueBusyWorkerEntry(ConcurrentQueue<TaskQueueWorkerEntry> sourceQueue, TaskQueueWorkerEntry worker)
        {
            if (sourceQueue == null)
            {
                throw new ArgumentNullException(nameof(sourceQueue), "The source queue can't be null.");
            }
            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker), "Worker can't be null.");
            }

            SourceQueue = sourceQueue;
            Worker = worker;
        }
        #endregion

        #region Public class methods

        public void Release()
        {
            SourceQueue.Enqueue(Worker);
        }
        #endregion
    }
}