using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueClientEntry : IDisposable
    {
        #region Constants and fields

        public static readonly TaskQueueClientEntry Empty;

        public readonly Guid ClientID;

        public readonly bool OnlyOwnEvents;

        public readonly TaskQueueServiceEvents SubscribedEvents;

        private readonly ITaskQueueClient Client;
        #endregion

        #region Properties

        public bool IsDisposed { get; private set; }
        #endregion

        #region Constructors

        static TaskQueueClientEntry()
        {
            Empty = new TaskQueueClientEntry();
        }

        private TaskQueueClientEntry()
        {
            IsDisposed = false;
        }

        public TaskQueueClientEntry(Guid clientID, ITaskQueueClient client) : this()
        {
            ClientID = clientID;

            OnlyOwnEvents = client.OnlyOwnEvents;
            SubscribedEvents = client.SubscribedEvents;
            Client = client;
        }
        #endregion

        #region Public class methods

        public void EventOccured(TaskQueueEventInfo eventInfo)
        {
            Client.EventOccured(eventInfo.EventType, eventInfo.EventData);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public void Close()
        {
            Client.Dispose();
        }
        #endregion
    }
}