using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Communication
{
    using Service.Infrastructure;

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    internal sealed class TaskQueueWorkerService : ITaskQueueWorkerService
    {
        #region Constants and fields

        private TaskQueueServiceImplementor Implementor;
        #endregion

        #region Constructors

        public TaskQueueWorkerService(TaskQueueServiceImplementor implementor)
        {
            Implementor = implementor;
        }
        #endregion

        #region ITaskQueueWorkerService implementation

        public bool RegisterWorker(Guid workerID, string[] taskCategories)
        {
            ITaskQueueWorkerCallback callback = OperationContext.Current.GetCallbackChannel<ITaskQueueWorkerCallback>();
            TaskQueueWorker worker = new TaskQueueWorker(callback);

            bool result = Implementor.RegisterWorker(workerID, worker, taskCategories);
            return result;
        }

        public void KeepAlive(Guid workerID)
        {
            Implementor.KeepAlive(workerID);
        }

        public void AcknowledgeTask(Guid workerID, Guid taskID)
        {
            Implementor.AcknowledgeTask(workerID, taskID);
        }

        public bool UnregisterWorker(Guid workerID)
        {
            bool result = Implementor.UnregisterWorker(workerID);
            return result;
        }
        #endregion
    }
}