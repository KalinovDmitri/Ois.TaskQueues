using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Communication
{
    [ServiceContract(Name = "TaskQueueWorkerCallback", Namespace = TaskQueueServiceConstants.Namespace)]
    public interface ITaskQueueWorkerCallback
    {

        [OperationContract]
        bool AssignTask(Guid taskID, string taskCategory, string taskData);
    }
}