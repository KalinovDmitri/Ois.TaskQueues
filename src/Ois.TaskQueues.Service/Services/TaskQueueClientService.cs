using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Communication
{
    using Service.Infrastructure;

    [ServiceBehavior(
        AutomaticSessionShutdown = false,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.PerCall,
        UseSynchronizationContext = false)]
    internal sealed class TaskQueueClientService : ITaskQueueClientService
    {
        #region Constants and fields

        private TaskQueueServiceImplementor Implementor;
        #endregion

        #region Constructors

        public TaskQueueClientService(TaskQueueServiceImplementor implementor)
        {
            Implementor = implementor;
        }
        #endregion

        #region ITaskQueueClientService implementation

        public bool RegisterClient(Guid clientID, bool onlyOwnEvents, TaskQueueServiceEvents subscribedEvents)
        {
            ITaskQueueClientCallback callback = OperationContext.Current.GetCallbackChannel<ITaskQueueClientCallback>();
            TaskQueueClient client = new TaskQueueClient(callback, onlyOwnEvents, subscribedEvents);
            
            bool result = Implementor.RegisterClient(clientID, client);
            return result;
        }

        public Guid CreateComputation(Guid clientID)
        {
            Guid computationID = Implementor.CreateComputation(clientID);
            return computationID;
        }

        public Guid AddTask(Guid computationID, string taskCategory, string taskData)
        {
            Guid taskID = Implementor.AddTask(computationID, taskCategory, taskData);
            return taskID;
        }

        public Guid AddBarrier(Guid computationID)
        {
            Guid barrierID = Implementor.AddBarrier(computationID);
            return barrierID;
        }

        public void FinishComputation(Guid computationID)
        {
            Implementor.FinishComputation(computationID);
        }

        public bool UnregisterClient(Guid clientID)
        {
            bool result = Implementor.UnregisterClient(clientID);
            return result;
        }
        #endregion
    }
}