using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueQueueService
    {
        #region Constants and fields
        
        private readonly ConcurrentDictionary<Guid, TaskQueueQueueEntry> Queues;
        #endregion

        #region Constructors

        public TaskQueueQueueService()
        {
            Queues = new ConcurrentDictionary<Guid, TaskQueueQueueEntry>();
        }
        #endregion

        #region Public class methods

        public TaskQueueQueueEntry CreateQueue(Guid clientID)
        {
            Guid queueID = Guid.NewGuid();

            TaskQueueQueueEntry queue = new TaskQueueQueueEntry(clientID, queueID);

            bool isAdded = Queues.TryAdd(queueID, queue);
            
            return (isAdded) ? queue : null;
        }

        public TaskQueueTaskEntry AddTask(Guid queueID, string taskCategory, string taskData)
        {
            TaskQueueTaskEntry taskEntry = null;

            TaskQueueQueueEntry queue = null;
            bool isExists = Queues.TryGetValue(queueID, out queue);
            if (isExists)
            {
                taskEntry = queue.AddTask(taskCategory, taskData);
            }

            return taskEntry;
        }

        public TaskQueueBarrierEntry AddBarrier(Guid queueID)
        {
            TaskQueueBarrierEntry barrier = null;
            
            TaskQueueQueueEntry queue = null;
            bool isExists = Queues.TryGetValue(queueID, out queue);
            if (isExists)
            {
                barrier = queue.AddBarrier();
            }

            return barrier;
        }

        public TaskQueueQueueEntry RemoveQueue(Guid queueID)
        {
            TaskQueueQueueEntry queue = null;

            bool isRemoved = Queues.TryRemove(queueID, out queue);

            return (isRemoved) ? queue : null;
        }
        #endregion
    }
}