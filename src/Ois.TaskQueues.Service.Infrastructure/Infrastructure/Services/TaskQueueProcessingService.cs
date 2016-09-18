using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueProcessingService
    {
        #region Constants and fields

        private readonly TaskQueueNotificationService NotificationService;

        private readonly TaskQueueWorkerService WorkerService;

        private readonly ConcurrentDictionary<Guid, TaskQueueProcessedTaskEntry> ProcessedTasks; // dictionary TaskID <--> ProcessedTaskEntry

        private readonly ConcurrentDictionary<Guid, TaskQueueQueueProcessor> Processors; // dictionary QueueID <--> QueueProcessor
        #endregion

        #region Properties

        public TaskQueueServiceConfiguration Configuration { get; set; }
        #endregion

        #region Constructors

        private TaskQueueProcessingService()
        {
            ProcessedTasks = new ConcurrentDictionary<Guid, TaskQueueProcessedTaskEntry>();
            Processors = new ConcurrentDictionary<Guid, TaskQueueQueueProcessor>();
        }

        public TaskQueueProcessingService(TaskQueueNotificationService notificationService, TaskQueueWorkerService workerService) : this()
        {
            NotificationService = notificationService;
            WorkerService = workerService;
        }
        #endregion

        #region Public class methods

        public void AddProcessor(TaskQueueQueueEntry queue)
        {
            Guid queueID = queue.QueueID;

            var processor = new TaskQueueQueueProcessor(WorkerService, queue);
            bool isAdded = Processors.TryAdd(queueID, processor);
            if (isAdded)
            {
                processor.TaskAssigned += new EventHandler<TaskQueueTaskAssignedEventArgs>(ProcessorTaskAssigned);

                processor.Start();
            }
        }

        public bool AcknowledgeTask(Guid workerID, Guid taskID)
        {
            WorkerService.ReleaseWorker(workerID);

            TaskQueueProcessedTaskEntry processedEntry = null;
            bool isFinished = false;
            bool isRemoved = ProcessedTasks.TryRemove(taskID, out processedEntry);
            if (isRemoved)
            {
                processedEntry.StopTimer();

                int remainingTasksCount = 0, processedTasksCount = 0;
                TaskQueueTaskEntry taskEntry = processedEntry.TaskEntry;

                isFinished = FinishTask(taskEntry, out remainingTasksCount, out processedTasksCount);
                if (isFinished)
                {
                    NotificationService.TaskFinished(taskEntry.ClientID, taskEntry.QueueID, taskEntry.TaskID, workerID);

                    bool isBarrierFinished = TryRemoveBarrierIfFinished(taskEntry.QueueID, taskEntry.BarrierID);
                    if (isBarrierFinished)
                    {
                        NotificationService.BarrierFinished(taskEntry.ClientID, taskEntry.QueueID, taskEntry.BarrierID);
                    }

                    if ((remainingTasksCount == 0) && (processedTasksCount == 0))
                    {
                        NotificationService.QueueEmptied(taskEntry.ClientID, taskEntry.QueueID);
                    }
                }
            }
            return isRemoved && isFinished;
        }

        public void RemoveProcessor(Guid queueID)
        {
            TaskQueueQueueProcessor processor = null;
            bool isRemoved = Processors.TryRemove(queueID, out processor);
            if (isRemoved)
            {
                processor.Stop();
            }
        }
        #endregion

        #region Private class methods

        private void ProcessorTaskAssigned(object sender, TaskQueueTaskAssignedEventArgs args)
        {
            Guid workerID = args.WorkerID;

            TaskQueueTaskEntry taskEntry = args.Task;
            NotificationService.TaskAssigned(taskEntry.ClientID, taskEntry.QueueID, taskEntry.TaskID, workerID);

            TimeSpan executionTimeout = Configuration.ExecutionTimeout;

            var processedEntry = new TaskQueueProcessedTaskEntry(workerID, taskEntry, executionTimeout);
            bool isAdded = ProcessedTasks.TryAdd(taskEntry.TaskID, processedEntry);
            if (isAdded)
            {
                processedEntry.TimeExceeded += new EventHandler(TaskExecutionTimeExceeded);
                processedEntry.StartTimer();
            }
        }

        private void TaskExecutionTimeExceeded(object sender, EventArgs args)
        {
            var processedEntry = sender as TaskQueueProcessedTaskEntry;
            if (processedEntry != null)
            {
                processedEntry.StopTimer();
                
                Guid queueID = processedEntry.TaskEntry.QueueID;
                TaskQueueQueueProcessor processor = null;
                bool isExists = Processors.TryGetValue(queueID, out processor);
                if (isExists)
                {
                    processor.EnqueueTask(processedEntry.TaskEntry);
                }
            }
        }

        private bool FinishTask(TaskQueueTaskEntry taskEntry, out int remainingTasksCount, out int processedTasksCount)
        {
            bool result = false;

            Guid queueID = taskEntry.QueueID;

            TaskQueueQueueProcessor processor = null;
            bool isExists = Processors.TryGetValue(queueID, out processor);
            if (isExists)
            {
                result = processor.FinishTask(taskEntry);
            }

            remainingTasksCount = processor.TasksCount;
            processedTasksCount = processor.ProcessedTasksCount;

            return result;
        }

        private bool TryRemoveBarrierIfFinished(Guid queueID, Guid barrierID)
        {
            bool result = false;

            TaskQueueQueueProcessor processor = null;
            bool isExists = Processors.TryGetValue(queueID, out processor);
            if (isExists)
            {
                result = processor.TryRemoveBarrierIfFinished(barrierID);
            }

            return result;
        }
        #endregion
    }
}