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

        public TaskQueueBalancingService BalancingService { get; set; }

        public TaskQueueClientService ClientService { get; set; }

        public TaskQueueQueueService QueueService { get; set; }

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

        public Guid CreateQueue(Guid clientID)
        {
            Logger.Debug($"CreateQueue request: ClientID = {{{clientID}}}");

            Guid queueID = Guid.Empty;

            TaskQueueQueueEntry queue = QueueService.CreateQueue(clientID);
            if (queue != null)
            {
                NotificationService.QueueCreated(clientID, queue.QueueID);

                ProcessingService.AddProcessor(queue);
                queueID = queue.QueueID;
            }

            Logger.Debug($"CreateQueue response: QueueID = {{{queueID}}}");

            return queueID;
        }

        public Guid AddTask(Guid queueID, string taskCategory, string taskData)
        {
            Logger.Debug($"AddTask request: QueueID = {{{queueID}}}; Category = {taskCategory}");

            Guid taskID = Guid.Empty;

            TaskQueueTaskEntry taskEntry = QueueService.AddTask(queueID, taskCategory, taskData);
            if (taskEntry != null)
            {
                taskID = taskEntry.TaskID;
                NotificationService.TaskAdded(taskEntry.ClientID, taskEntry.QueueID, taskEntry.TaskID);
                BalancingService.TaskAdded();
            }

            Logger.Debug($"AddTask response: TaskID = {{{taskID}}}");

            return taskID;
        }

        public Guid AddBarrier(Guid queueID)
        {
            Logger.Debug($"AddBarrier request; QueueID = {{{queueID}}}");

            Guid barrierID = Guid.Empty;

            TaskQueueBarrierEntry barrier = QueueService.AddBarrier(queueID);
            if (barrier != null)
            {
                NotificationService.BarrierAdded(barrier.ClientID, barrier.QueueID, barrier.BarrierID);
                barrierID = barrier.BarrierID;
            }

            Logger.Debug($"AddBarrier response: Barrier ID = {{{barrierID}}}");

            return barrierID;
        }

        public void RemoveQueue(Guid queueID)
        {
            Logger.Debug($"Enter RemoveQueue: QueueID = {{{queueID}}}");

            TaskQueueQueueEntry queue = QueueService.RemoveQueue(queueID);
            if (queue != null)
            {
                NotificationService.QueueRemoved(queue.ClientID, queue.QueueID);
                ProcessingService.RemoveProcessor(queue.QueueID);
            }

            Logger.Debug("Exit RemoveQueue");
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
                BalancingService.WorkerConnected();
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

            bool isFinished = ProcessingService.AcknowledgeTask(workerID, taskID);
            if (isFinished)
            {
                BalancingService.TaskFinished();
            }

            Logger.Debug("Exit AcknowledgeTask");
        }

        public bool UnregisterWorker(Guid workerID)
        {
            Logger.Debug($"UnregisterWorker request: WorkerID = {{{workerID}}}");

            bool result = WorkerService.RemoveWorker(workerID);
            if (result)
            {
                NotificationService.WorkerDisconnected(workerID);
                BalancingService.WorkerDisconnected();
            }

            Logger.Debug($"UnregisterWorker response: result = {result}");

            return result;
        }
        #endregion
    }
}