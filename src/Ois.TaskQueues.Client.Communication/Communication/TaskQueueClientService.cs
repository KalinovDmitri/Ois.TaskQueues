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

        public Guid CreateComputation(Guid clientID)
        {
            Guid computationID = Channel.CreateComputation(clientID);
            return computationID;
        }

        public Guid AddTask(Guid computationID, string taskCategory, string taskData)
        {
            Guid taskID = Channel.AddTask(computationID, taskCategory, taskData);
            return taskID;
        }

        public Guid AddBarrier(Guid computationID)
        {
            Guid barrierID = Channel.AddBarrier(computationID);
            return barrierID;
        }

        public void FinishComputation(Guid computationID)
        {
            Channel.FinishComputation(computationID);
        }

        public bool UnregisterClient(Guid clientID)
        {
            bool result = Channel.UnregisterClient(clientID);
            return result;
        }
        #endregion
    }
}