using System;

using Autofac;
using NLog;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueServiceImplementor
    {
        #region Constants and fields

        private readonly ILogger Logger;
        #endregion

        #region Properties

        public TaskQueueClientService ClientService { get; set; }

        public TaskQueueComputationService ComputationService { get; set; }

        public TaskQueueWorkerService WorkerService { get; set; }

        public TaskQueueNotificationService NotificationService { get; set; }

        public TaskQueueProcessingService ProcessingService { get; set; }
        #endregion

        #region Constructors

        public TaskQueueServiceImplementor(ILogger logger)
        {
            Logger = logger;
        }
        #endregion

        #region Client service methods

        public bool RegisterClient(Guid clientID, ITaskQueueClient client)
        {
            Logger.Debug($"RegisterClient request: ClientID = {{{clientID}}}");

            TaskQueueClientEntry clientEntry = ClientService.RegisterClient(clientID, client);

            bool clientCreated = clientEntry.ClientID != Guid.Empty;
            if (clientCreated)
            {
                NotificationService.ClientConnected(clientEntry);
            }
            
            Logger.Debug($"RegisterClient response: result = {clientCreated}");

            return clientCreated;
        }

        public Guid CreateComputation(Guid clientID)
        {
            Logger.Debug($"CreateComputation request: ClientID = {{{clientID}}}");

            Guid computationID = Guid.Empty;

            TaskQueueComputationEntry computation = ComputationService.CreateComputation(clientID);
            if (computation != null)
            {
                NotificationService.ComputationCreated(clientID, computation.ComputationID);

                ProcessingService.AddProcessor(computation);
                computationID = computation.ComputationID;
            }

            Logger.Debug($"CreateComputation response: ComputationID = {{{computationID}}}");

            return computationID;
        }

        public Guid AddTask(Guid computationID, string taskCategory, string taskData)
        {
            Logger.Debug($"AddTask request: ComputationID = {{{computationID}}}; category = {taskCategory}");

            Guid taskID = Guid.Empty;

            TaskQueueTaskEntry taskEntry = ComputationService.AddTask(computationID, taskCategory, taskData);
            if (taskEntry != null)
            {
                NotificationService.TaskAdded(taskEntry.ClientID, taskEntry.ComputationID, taskEntry.TaskID);
                taskID = taskEntry.TaskID;
            }

            Logger.Debug($"AddTask response: TaskID = {{{taskID}}}");

            return taskID;
        }

        public Guid AddBarrier(Guid computationID)
        {
            Logger.Debug($"AddBarrier request; Computation ID = {{{computationID}}}");

            Guid barrierID = Guid.Empty;

            TaskQueueBarrierEntry barrier = ComputationService.AddBarrier(computationID);
            if (barrier != null)
            {
                NotificationService.BarrierAdded(barrier.ClientID, barrier.ComputationID, barrier.BarrierID);
                barrierID = barrier.BarrierID;
            }

            Logger.Debug($"AddBarrier response: Barrier ID = {{{barrierID}}}");

            return barrierID;
        }

        public void FinishComputation(Guid computationID)
        {
            Logger.Debug($"Enter FinishComputation: ComputationID = {{{computationID}}}");

            TaskQueueComputationEntry computation = ComputationService.FinishComputation(computationID);
            if (computation != null)
            {
                ProcessingService.RemoveProcessor(computation.ComputationID);
            }

            Logger.Debug("Exit FinishComputation");
        }

        public bool UnregisterClient(Guid clientID)
        {
            Logger.Debug($"UnregisterClient request: ClientID = {{{clientID}}}");

            bool result = ClientService.UnregisterClient(clientID);

            if (result)
            {
                NotificationService.ClientDisconnected(clientID);
            }

            Logger.Debug($"UnregisterClient response: result = {result}");

            return result;
        }
        #endregion

        #region Worker service methods

        public bool RegisterWorker(Guid workerID, ITaskQueueWorker worker, string[] taskCategories)
        {
            Logger.Debug($"RegisterWorker request: WorkerID = {{{workerID}}}");

            bool result = WorkerService.AddWorker(workerID, worker, taskCategories);

            if (result)
            {
                NotificationService.WorkerConnected(workerID, taskCategories);
            }

            Logger.Debug($"RegisterWorker response: result = {result}");

            return result;
        }

        public void KeepAlive(Guid workerID)
        {
            Logger.Debug($"Enter KeepAlive; WorkerID = {{{workerID}}}");

            WorkerService.KeepAliveWorker(workerID);

            Logger.Debug("Exit KeepAlive");
        }

        public void AcknowledgeTask(Guid workerID, Guid taskID)
        {
            Logger.Debug($"Enter AcknowledgeTask;\r\nWorkerID = {{{workerID}}}\r\nTaskID = {{{taskID}}}");

            ProcessingService.AcknowledgeTask(workerID, taskID);

            Logger.Debug("Exit AcknowledgeTask");
        }

        public bool UnregisterWorker(Guid workerID)
        {
            Logger.Debug($"UnregisterWorker request: WorkerID = {{{workerID}}}");

            bool result = WorkerService.RemoveWorker(workerID);

            if (result)
            {
                NotificationService.WorkerDisconnected(workerID);
            }

            Logger.Debug($"UnregisterWorker response: result = {result}");

            return result;
        }
        #endregion
    }
}