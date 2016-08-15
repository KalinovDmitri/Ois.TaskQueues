using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public interface ITaskQueueClient : IDisposable
    {
        bool OnlyOwnEvents { get; }

        TaskQueueServiceEvents SubscribedEvents { get; }

        void EventOccured(TaskQueueServiceEvents occuredEvent, string eventData);
    }
}