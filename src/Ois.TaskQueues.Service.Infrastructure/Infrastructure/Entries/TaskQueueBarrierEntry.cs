using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueBarrierEntry
    {
        #region Constants and fields

        public readonly Guid ClientID;

        public readonly Guid QueueID;

        public readonly Guid BarrierID;

        private readonly ConcurrentQueue<TaskQueueTaskEntry> TasksQueue;

        private readonly ConcurrentDictionary<Guid, TaskQueueTaskEntry> ProcessedTasks;

        private int CountOfTasks = 0;

        private int CountOfProcessedTasks = 0;
        #endregion

        #region Properties

        public int TasksCount => CountOfTasks;

        public int ProcessedTasksCount => CountOfProcessedTasks;
        #endregion

        #region Constructors

        private TaskQueueBarrierEntry()
        {
            TasksQueue = new ConcurrentQueue<TaskQueueTaskEntry>();
            ProcessedTasks = new ConcurrentDictionary<Guid, TaskQueueTaskEntry>();
        }

        public TaskQueueBarrierEntry(Guid clientID, Guid queueID, Guid barrierID) : this()
        {
            ClientID = clientID;
            QueueID = queueID;
            BarrierID = barrierID;
        }
        #endregion

        #region object methods overriding

        public override bool Equals(object obj)
        {
            TaskQueueBarrierEntry other = obj as TaskQueueBarrierEntry;

            return (other != null) && (QueueID == other.QueueID) && (BarrierID == other.BarrierID);
        }

        public override int GetHashCode()
        {
            return BarrierID.GetHashCode();
        }

        public override string ToString()
        {
            string identifier = BarrierID.ToString("B").ToUpperInvariant();

            return $"Barrier {identifier}: {CountOfTasks} tasks";
        }
        #endregion

        #region Public class methods

        public void AddTask(TaskQueueTaskEntry taskEntry)
        {
            TasksQueue.Enqueue(taskEntry);
            Interlocked.Increment(ref CountOfTasks);
        }

        public void AddTaskToProcessed(TaskQueueTaskEntry taskEntry)
        {
            bool isAdded = ProcessedTasks.TryAdd(taskEntry.TaskID, taskEntry);
            if (isAdded)
            {
                Interlocked.Increment(ref CountOfProcessedTasks);
            }
        }

        public bool TryDequeueTask(out TaskQueueTaskEntry taskEntry)
        {
            bool result = TasksQueue.TryDequeue(out taskEntry);
            if (result)
            {
                Interlocked.Decrement(ref CountOfTasks);
            }
            return result;
        }

        public void EnqueueTask(TaskQueueTaskEntry taskEntry)
        {
            AddTask(taskEntry);
            RemoveTaskFromProcessed(taskEntry.TaskID);
        }

        public bool RemoveTaskFromProcessed(Guid taskID)
        {
            TaskQueueTaskEntry removedTask = null;
            bool isRemoved = ProcessedTasks.TryRemove(taskID, out removedTask);
            if (isRemoved)
            {
                Interlocked.Decrement(ref CountOfProcessedTasks);
            }
            return isRemoved;
        }
        #endregion
    }
}