using System;

namespace Ois.TaskQueues.Worker
{
    public sealed class TaskQueueTaskInfo
    {
        #region Constants and fields

        public readonly Guid TaskID;

        public readonly string TaskData;
        #endregion

        #region Constructors

        public TaskQueueTaskInfo(Guid taskID, string taskData)
        {
            TaskID = taskID; TaskData = taskData;
        }
        #endregion
    }
}