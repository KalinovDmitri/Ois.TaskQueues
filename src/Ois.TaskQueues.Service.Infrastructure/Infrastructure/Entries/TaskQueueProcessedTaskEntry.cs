using System;
using System.Threading;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueProcessedTaskEntry
    {
        #region Constants and fields

        public readonly Guid WorkerID;

        public readonly TaskQueueTaskEntry TaskEntry;

        private readonly TimeSpan ExecutionTimeout;

        private Timer ExecutionTimer;
        #endregion

        #region Events

        public event EventHandler TimeExceeded;
        #endregion

        #region Constructors

        public TaskQueueProcessedTaskEntry(Guid workerID, TaskQueueTaskEntry taskEntry, TimeSpan executionTimeout)
        {
            WorkerID = workerID; TaskEntry = taskEntry; ExecutionTimeout = executionTimeout;
        }
        #endregion

        #region object methods overriding

        public override bool Equals(object obj)
        {
            TaskQueueProcessedTaskEntry other = obj as TaskQueueProcessedTaskEntry;

            return (other != null) && (WorkerID == other.WorkerID) && (TaskEntry.TaskID == other.TaskEntry.TaskID);
        }

        public override int GetHashCode()
        {
            int workerCode = WorkerID.GetHashCode();
            int taskCode = TaskEntry.GetHashCode();

            return workerCode ^ (5 + taskCode);
        }

        public override string ToString()
        {
            string workerID = WorkerID.ToString("B").ToUpperInvariant();

            return $"Worker: {workerID}; {TaskEntry}";
        }
        #endregion

        #region Public class methods

        public void StartTimer()
        {
            ExecutionTimer = new Timer(ExecutionTimeExceeded, null, ExecutionTimeout, Timeout.InfiniteTimeSpan);
        }

        public void StopTimer()
        {
            ExecutionTimer.Dispose();
        }
        #endregion

        #region Private class methods

        private void ExecutionTimeExceeded(object state)
        {
            TimeExceeded?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}