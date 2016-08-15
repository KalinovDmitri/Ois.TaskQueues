using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueComputationEntry
    {
        #region Constants and fields

        public readonly Guid ClientID;

        public readonly Guid ComputationID;

        private readonly Func<Guid, TaskQueueBarrierEntry> BarrierCreator;

        private readonly ConcurrentQueue<Guid> BarriersQueue;

        private readonly ConcurrentDictionary<Guid, TaskQueueBarrierEntry> Barriers;

        private Guid BarrierID;

        private int CountOfTasks = 0;
        #endregion

        #region Events

        public event EventHandler<TaskQueueBarrierEntry> BarrierFinished;
        #endregion

        #region Properties

        public int TasksCount => CountOfTasks;
        #endregion

        #region Constructors
        
        private TaskQueueComputationEntry()
        {
            BarrierID = Guid.NewGuid();

            BarrierCreator = new Func<Guid, TaskQueueBarrierEntry>(CreateBarrierEntry);
            Barriers = new ConcurrentDictionary<Guid, TaskQueueBarrierEntry>();

            BarriersQueue = new ConcurrentQueue<Guid>();
        }

        public TaskQueueComputationEntry(Guid clientID, Guid computationID) : this()
        {
            ClientID = clientID;
            ComputationID = computationID;
        }
        #endregion

        #region object methods overriding

        public override bool Equals(object obj)
        {
            TaskQueueComputationEntry other = obj as TaskQueueComputationEntry;

            return (other != null) && (ComputationID == other.ComputationID);
        }

        public override int GetHashCode()
        {
            return ComputationID.GetHashCode();
        }

        public override string ToString()
        {
            string identifier = ComputationID.ToString("B").ToUpperInvariant();

            return $"Computation {identifier}: {CountOfTasks} tasks";
        }
        #endregion

        #region Public class methods

        public TaskQueueTaskEntry AddTask(string taskCategory, string taskData)
        {
            TaskQueueBarrierEntry barrierEntry = Barriers.GetOrAdd(BarrierID, BarrierCreator);

            TaskQueueTaskEntry taskEntry = new TaskQueueTaskEntry(ClientID, ComputationID, BarrierID, taskCategory, taskData);

            barrierEntry.AddTask(taskEntry);

            Interlocked.Increment(ref CountOfTasks);

            return taskEntry;
        }

        public void AddTaskToProcessed(TaskQueueTaskEntry taskEntry)
        {
            TaskQueueBarrierEntry barrierEntry = Barriers.GetOrAdd(taskEntry.BarrierID, BarrierCreator);

            barrierEntry.AddTaskToProcessed(taskEntry);
        }

        public TaskQueueBarrierEntry AddBarrier()
        {
            Guid currentBarrierID = BarrierID;

            BarriersQueue.Enqueue(currentBarrierID);

            BarrierID = Guid.NewGuid();

            return Barriers.GetOrAdd(currentBarrierID, BarrierCreator);
        }

        public bool TryDequeueTask(out TaskQueueTaskEntry taskEntry)
        {
            Guid barrierID = GetCurrentBarrier();
            TaskQueueBarrierEntry barrierEntry = Barriers.GetOrAdd(barrierID, BarrierCreator);

            bool result = false;

            if (barrierEntry.TasksCount > 0)
            {
                result = barrierEntry.TryDequeueTask(out taskEntry);
            }
            else taskEntry = null;

            if (result)
            {
                Interlocked.Decrement(ref CountOfTasks);
            }

            return result;
        }

        public void EnqueueTask(TaskQueueTaskEntry taskEntry)
        {
            Guid barrierID = taskEntry.BarrierID;

            TaskQueueBarrierEntry barrierEntry = Barriers.GetOrAdd(barrierID, BarrierCreator);

            barrierEntry.EnqueueTask(taskEntry);

            Interlocked.Increment(ref CountOfTasks);
        }

        public bool FinishTask(TaskQueueTaskEntry taskEntry)
        {
            bool result = false;

            TaskQueueBarrierEntry barrierEntry = Barriers.GetOrAdd(taskEntry.BarrierID, BarrierCreator);
            barrierEntry.RemoveTaskFromProcessed(taskEntry.TaskID);

            if ((barrierEntry.TasksCount == 0) && (barrierEntry.ProcessedTasksCount == 0))
            {
                RemoveFinishedBarrier(barrierEntry.BarrierID);
                result = true;
            }
            else result = true;

            return result;
        }
        #endregion

        #region Internal & private class methods

        internal Guid GetCurrentBarrier()
        {
            Guid result = Guid.Empty;

            bool hasItems = BarriersQueue.TryPeek(out result);

            return (hasItems) ? result : BarrierID;
        }
        
        private TaskQueueBarrierEntry CreateBarrierEntry(Guid barrierID)
        {
            return new TaskQueueBarrierEntry(ClientID, ComputationID, barrierID);
        }

        private void RemoveFinishedBarrier(Guid barrierID)
        {
            Guid finishedBarrierID = Guid.Empty;
            BarriersQueue.TryDequeue(out finishedBarrierID);

            TaskQueueBarrierEntry completedBarrier = null;
            bool isRemoved = Barriers.TryRemove(barrierID, out completedBarrier);
            if (isRemoved)
            {
                OnBarrierFinished(completedBarrier);
            }
        }

        private void OnBarrierFinished(TaskQueueBarrierEntry barrierEntry)
        {
            BarrierFinished?.Invoke(this, barrierEntry);
        }
        #endregion
    }
}