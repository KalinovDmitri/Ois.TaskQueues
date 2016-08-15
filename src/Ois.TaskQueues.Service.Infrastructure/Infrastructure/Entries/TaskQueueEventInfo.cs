using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueEventInfo
    {
        #region Constants and fields

        public readonly Guid ClientID;

        public readonly TaskQueueServiceEvents EventType;

        public readonly string EventData;
        #endregion

        #region Constructors

        public TaskQueueEventInfo(Guid clientID, TaskQueueServiceEvents eventType, string eventData)
        {
            ClientID = clientID;
            EventType = eventType;
            EventData = eventData;
        }
        #endregion
    }
}