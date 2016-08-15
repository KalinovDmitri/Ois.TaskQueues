using System;

namespace Ois.TaskQueues
{

    public sealed class TaskQueueEventOccuredEventArgs
    {
        #region Constants and fields

        public readonly TaskQueueServiceEvents EventType;

        public readonly string EventData;
        #endregion

        #region Constructors

        public TaskQueueEventOccuredEventArgs(TaskQueueServiceEvents eventType, string eventData)
        {
            EventType = eventType; EventData = eventData;
        }
        #endregion
    }
}