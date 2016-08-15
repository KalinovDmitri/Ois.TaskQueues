using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueComputationService
    {
        #region Constants and fields
        
        private readonly ConcurrentDictionary<Guid, TaskQueueComputationEntry> Computations;

        private readonly Func<Guid, TaskQueueComputationEntry> ComputationCreator;
        #endregion

        #region Constructors

        public TaskQueueComputationService()
        {
            Computations = new ConcurrentDictionary<Guid, TaskQueueComputationEntry>();

            ComputationCreator = new Func<Guid, TaskQueueComputationEntry>(CreateComputationEntry);
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
            TaskQueueComputationEntry computation = Computations.GetOrAdd(computationID, ComputationCreator);
            
            TaskQueueTaskEntry taskEntry = computation.AddTask(taskCategory, taskData);

            return taskEntry;
        }

        public TaskQueueBarrierEntry AddBarrier(Guid computationID)
        {
            TaskQueueComputationEntry computation = Computations.GetOrAdd(computationID, ComputationCreator);

            TaskQueueBarrierEntry barrier = computation.AddBarrier();

            return barrier;
        }

        public TaskQueueComputationEntry FinishComputation(Guid computationID)
        {
            TaskQueueComputationEntry computation = null;

            bool isRemoved = Computations.TryRemove(computationID, out computation);

            return (isRemoved) ? computation : null;
        }
        #endregion

        #region Private class methods

        private TaskQueueComputationEntry CreateComputationEntry(Guid computationID)
        {
            return new TaskQueueComputationEntry(Guid.Empty, computationID);
        }
        #endregion
    }
}