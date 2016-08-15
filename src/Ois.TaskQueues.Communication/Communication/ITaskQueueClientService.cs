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
        Guid CreateComputation(Guid clientID);

        [OperationContract]
        Guid AddTask(Guid computationID, string taskCategory, string taskData);

        [OperationContract]
        Guid AddBarrier(Guid computationID);

        [OperationContract(IsOneWay = true, IsTerminating = false)]
        void FinishComputation(Guid computationID);

        [OperationContract]
        bool UnregisterClient(Guid clientID);
    }
}