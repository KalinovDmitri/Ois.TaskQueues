using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public abstract class TaskQueueItemEntry
    {
        #region Constants and fields

        public readonly TaskQueueItemType ItemType;
        #endregion

        #region Constructors

        protected internal TaskQueueItemEntry(TaskQueueItemType itemType)
        {
            ItemType = itemType;
        }
        #endregion
    }
}