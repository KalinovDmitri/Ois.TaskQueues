using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueComputationService
    {
        #region Constants and fields
        
        private readonly ConcurrentDictionary<Guid, TaskQueueComputationEntry> Computations;
        #endregion

        #region Constructors

        public TaskQueueComputationService()
        {
            Computations = new ConcurrentDictionary<Guid, TaskQueueComputationEntry>();
        }
        #endregion

        #region Public class methods

        public TaskQueueComputationEntry CreateComputation(Guid clientID)
        {
            Guid computationID = Guid.NewGuid();

            TaskQueueComputationEntry computation = new TaskQueueComputationEntry(clientID, computationID);

            bool isAdded = Computations.TryAdd(computationID, computation);
            
            return (isAdded) ? computation : null;
        }

        public TaskQueueTaskEntry AddTask(Guid computationID, string taskCategory, string taskData)
        {
            TaskQueueTaskEntry taskEntry = null;

            TaskQueueComputationEntry computation = null;
            bool isExists = Computations.TryGetValue(computationID, out computation);
            if (isExists)
            {
                taskEntry = computation.AddTask(taskCategory, taskData);
            }

            return taskEntry;
        }

        public TaskQueueBarrierEntry AddBarrier(Guid computationID)
        {
            TaskQueueBarrierEntry barrier = null;
            
            TaskQueueComputationEntry computation = null;
            bool isExists = Computations.TryGetValue(computationID, out computation);
            if (isExists)
            {
                barrier = computation.AddBarrier();
            }

            return barrier;
        }

        public TaskQueueComputationEntry FinishComputation(Guid computationID)
        {
            TaskQueueComputationEntry computation = null;

            bool isRemoved = Computations.TryRemove(computationID, out computation);

            return (isRemoved) ? computation : null;
        }
        #endregion
    }
}