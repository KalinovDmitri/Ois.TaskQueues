using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueTaskAssignedEventArgs
    {
        #region Fields

        public readonly Guid WorkerID;

        public readonly TaskQueueTaskEntry Task;
        #endregion

        #region Constructors

        public TaskQueueTaskAssignedEventArgs(Guid workerID, TaskQueueTaskEntry task)
        {
            WorkerID = workerID;
            Task = task;
        }
        #endregion
    }
}