using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Ois.TaskQueues.Communication
{
    internal sealed class TaskQueueClientService : DuplexClientBase<ITaskQueueClientService>, ITaskQueueClientService
    {
        #region Constructors

        public TaskQueueClientService(InstanceContext callback, Binding binding, EndpointAddress serviceAddress) :
            base(callback, binding, serviceAddress)
        { }
        #endregion

        #region ITaskQueueClientService implementation

        public bool RegisterClient(Guid clientID, bool onlyOwnEvents, TaskQueueServiceEvents subscribedEvents)
        {
            bool result = Channel.RegisterClient(clientID, onlyOwnEvents, subscribedEvents);
            return result;
        }

        public Guid CreateQueue(Guid clientID)
        {
            Guid queueID = Channel.CreateQueue(clientID);
            return queueID;
        }

        public Guid AddTask(Guid queueID, string taskCategory, string taskData)
        {
            Guid taskID = Channel.AddTask(queueID, taskCategory, taskData);
            return taskID;
        }

        public Guid AddBarrier(Guid queueID)
        {
            Guid barrierID = Channel.AddBarrier(queueID);
            return barrierID;
        }

        public void RemoveQueue(Guid queueID)
        {
            Channel.RemoveQueue(queueID);
        }

        public bool UnregisterClient(Guid clientID)
        {
            bool result = Channel.UnregisterClient(clientID);
            return result;
        }
        #endregion
    }
}