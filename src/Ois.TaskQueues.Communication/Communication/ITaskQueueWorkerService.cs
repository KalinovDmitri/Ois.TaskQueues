using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Communication
{
    [ServiceContract(Name = "TaskQueueWorkerService", Namespace = TaskQueueServiceConstants.Namespace, CallbackContract = typeof(ITaskQueueWorkerCallback))]
    public interface ITaskQueueWorkerService
    {

        [OperationContract]
        bool RegisterWorker(Guid workerID, string[] taskCategories);

        [OperationContract(IsOneWay = true, IsTerminating = false)]
        void KeepAlive(Guid workerID);

        [OperationContract(IsOneWay = true, IsTerminating = false)]
        void AcknowledgeTask(Guid workerID, Guid taskID);

        [OperationContract]
        bool UnregisterWorker(Guid workerID);
    }
}