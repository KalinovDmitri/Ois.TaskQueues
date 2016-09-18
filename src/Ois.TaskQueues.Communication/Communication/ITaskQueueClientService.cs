using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Communication
{
    [ServiceContract(Name = "TaskQueueClientService", Namespace = TaskQueueServiceConstants.Namespace, CallbackContract = typeof(ITaskQueueClientCallback))]
    public interface ITaskQueueClientService
    {

        [OperationContract]
        bool RegisterClient(Guid clientID, bool onlyOwnEvents = true, TaskQueueServiceEvents subscribedEvents = TaskQueueServiceEvents.All);

        [OperationContract]
        Guid CreateQueue(Guid clientID);

        [OperationContract]
        Guid AddTask(Guid queueID, string taskCategory, string taskData);

        [OperationContract]
        Guid AddBarrier(Guid queueID);

        [OperationContract(IsOneWay = true, IsTerminating = false)]
        void RemoveQueue(Guid queueID);

        [OperationContract]
        bool UnregisterClient(Guid clientID);
    }
}