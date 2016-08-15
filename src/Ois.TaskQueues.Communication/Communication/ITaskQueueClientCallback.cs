using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Communication
{
    [ServiceContract(Name = "TaskQueueClientCallback", Namespace = TaskQueueServiceConstants.Namespace)]
    public interface ITaskQueueClientCallback
    {

        [OperationContract(IsOneWay = true, IsTerminating = false)]
        void EventOccured(TaskQueueServiceEvents occuredEvent, string eventData);
    }
}