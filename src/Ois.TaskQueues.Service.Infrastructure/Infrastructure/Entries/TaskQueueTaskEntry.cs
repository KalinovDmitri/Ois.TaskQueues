using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueTaskEntry
    {
        #region Constants and fields

        private const string EmptyTaskData = "{}";

        public readonly Guid ClientID;

        public readonly Guid QueueID;

        public readonly Guid BarrierID;

        public readonly Guid TaskID;

        public readonly string TaskCategory;

        public readonly string TaskData;
        #endregion

        #region Constructors

        private TaskQueueTaskEntry()
        {
            TaskID = Guid.NewGuid();
        }

        public TaskQueueTaskEntry(Guid clientID, Guid queueID, Guid barrierID, string taskCategory, string taskData) : this()
        {
            ClientID = clientID;
            QueueID = queueID;
            BarrierID = barrierID;
            TaskCategory = taskCategory;
            TaskData = taskData;
        }
        #endregion

        #region object methods overriding

        public override bool Equals(object obj)
        {
            TaskQueueTaskEntry other = obj as TaskQueueTaskEntry;

            return (other != null) && (QueueID == other.QueueID) && (TaskID == other.TaskID);
        }

        public override int GetHashCode()
        {
            return TaskID.GetHashCode();
        }

        public override string ToString()
        {
            string identifier = TaskID.ToString("B").ToUpperInvariant();

            return $"Task {identifier}: {TaskCategory}";
        }
        #endregion
    }
}