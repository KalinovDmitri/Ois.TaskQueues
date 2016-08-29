using System;
using System.Threading;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueWorkerEntry : IDisposable
    {
        #region Constants and fields

        private const int LockTimeout = 500; // 500 milliseconds

        private const int AvailabilityInterval = 30 * 1000; // 30 seconds

        private const double MaxAvailabilityDifference = 20.0; // 20 seconds

        private readonly object LockObject = new object();

        public readonly Guid WorkerID;

        private readonly ITaskQueueWorker WorkerCore;

        private DateTime LastAliveTime;

        private Timer AvailabilityTimer;
        #endregion

        #region Properties

        public bool IsDisposed { get; private set; }
        #endregion

        #region Constructors

        private TaskQueueWorkerEntry()
        {
            IsDisposed = false;
            LastAliveTime = DateTime.UtcNow;

            AvailabilityTimer = new Timer(CheckAvailability, null, AvailabilityInterval, AvailabilityInterval);
        }

        public TaskQueueWorkerEntry(Guid workerID, ITaskQueueWorker worker) : this()
        {
            WorkerID = workerID;
            WorkerCore = worker;
        }
        #endregion

        #region object methods overriding

        public override bool Equals(object obj)
        {
            TaskQueueWorkerEntry other = obj as TaskQueueWorkerEntry;

            return (other != null) && (WorkerID == other.WorkerID);
        }

        public override int GetHashCode()
        {
            return WorkerID.GetHashCode();
        }

        public override string ToString()
        {
            string identifier = WorkerID.ToString("B").ToUpperInvariant();

            return $"Worker {identifier}: {LastAliveTime}";
        }
        #endregion

        #region Public class methods

        public bool AssignTask(TaskQueueTaskEntry taskEntry)
        {
            bool isAssigned = false;
            bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
            if (isLocked)
            {
                isAssigned = WorkerCore.AssignTask(taskEntry.TaskID, taskEntry.TaskCategory, taskEntry.TaskData);
                Monitor.Exit(LockObject);
            }
            return isAssigned;
        }

        public void Close()
        {
            WorkerCore.Dispose();
        }

        public void Dispose()
        {
            bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
            if (isLocked)
            {
                AvailabilityTimer.Dispose();
                IsDisposed = true;
                Monitor.Exit(LockObject);
            }
        }

        public void UpdateAliveTime()
        {
            bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
            if (isLocked)
            {
                LastAliveTime = DateTime.UtcNow;
                Monitor.Exit(LockObject);
            }
        }
        #endregion

        #region Private class methods

        private void CheckAvailability(object state)
        {
            TimeSpan difference = DateTime.UtcNow - LastAliveTime;
            if (difference.TotalSeconds > MaxAvailabilityDifference)
            {
                Dispose();
            }
        }
        #endregion
    }
}