using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueTaskEntry
    {
        #region Constants and fields

        private const string EmptyTaskData = "{}";

        public readonly Guid ClientID;

        public readonly Guid BarrierID;

        public readonly Guid ComputationID;

        public readonly Guid TaskID;

        public readonly string TaskCategory;

        public readonly string TaskData;
        #endregion

        #region Constructors

        private TaskQueueTaskEntry()
        {
            TaskID = Guid.NewGuid();
        }

        public TaskQueueTaskEntry(Guid clientID, Guid computationID, Guid barrierID, string taskCategory, string taskData) : this()
        {
            ClientID = clientID;
            ComputationID = computationID;
            BarrierID = barrierID;
            TaskCategory = taskCategory;
            TaskData = taskData;
        }
        #endregion

        #region object methods overriding

        public override bool Equals(object obj)
        {
            TaskQueueTaskEntry other = obj as TaskQueueTaskEntry;

            return (other != null) && (ComputationID == other.ComputationID) && (TaskID == other.TaskID);
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