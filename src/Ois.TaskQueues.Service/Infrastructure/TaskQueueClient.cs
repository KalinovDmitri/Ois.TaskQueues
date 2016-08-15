using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Service.Infrastructure
{
    using Communication;

    internal class TaskQueueClient : ITaskQueueClient, IDisposable
    {
        #region Fields and properties

        private ITaskQueueClientCallback Callback;

        public bool OnlyOwnEvents { get; private set; }

        public TaskQueueServiceEvents SubscribedEvents { get; private set; }
        #endregion

        #region Constructors

        public TaskQueueClient(ITaskQueueClientCallback callback, bool onlyOwnEvents, TaskQueueServiceEvents subscribedEvents)
        {
            Callback = callback;
            OnlyOwnEvents = onlyOwnEvents;
            SubscribedEvents = subscribedEvents;
        }
        #endregion

        #region Public class methods

        public void EventOccured(TaskQueueServiceEvents occuredEvent, string eventData)
        {
            Callback.EventOccured(occuredEvent, eventData);
        }

        public void Dispose()
        {
            ICommunicationObject channel = Callback as ICommunicationObject;
            if (channel != null)
            {
                channel.Close();
            }
        }
        #endregion
    }
}