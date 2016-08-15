using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Ois.TaskQueues.Worker
{
    using Communication;

    internal sealed class TaskQueueWorkerService : DuplexClientBase<ITaskQueueWorkerService>, ITaskQueueWorkerService
    {
        #region Constructors

        public TaskQueueWorkerService(InstanceContext callback, Binding binding, EndpointAddress serviceAddress) :
            base(callback, binding, serviceAddress)
        { }
        #endregion

        #region ITaskQueueWorkerService implementation

        public bool RegisterWorker(Guid workerID, string[] taskCategories)
        {
            bool result = Channel.RegisterWorker(workerID, taskCategories);
            return result;
        }

        public void KeepAlive(Guid workerID)
        {
            Channel.KeepAlive(workerID);
        }

        public void AcknowledgeTask(Guid workerID, Guid taskID)
        {
            Channel.AcknowledgeTask(workerID, taskID);
        }

        public bool UnregisterWorker(Guid workerID)
        {
            bool result = Channel.UnregisterWorker(workerID);
            return result;
        }
        #endregion
    }
}