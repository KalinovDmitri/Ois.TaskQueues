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

        private readonly ConcurrentDictionary<Guid, TaskQueueComputationProcessor> Processors; // dictionary ComputationID <--> ComputationProcessor
        #endregion

        #region Properties

        public TaskQueueServiceConfiguration Configuration { get; set; }
        #endregion

        #region Constructors

        private TaskQueueProcessingService()
        {
            ProcessedTasks = new ConcurrentDictionary<Guid, TaskQueueProcessedTaskEntry>();
            Processors = new ConcurrentDictionary<Guid, TaskQueueComputationProcessor>();
        }

        public TaskQueueProcessingService(TaskQueueNotificationService notificationService, TaskQueueWorkerService workerService) : this()
        {
            NotificationService = notificationService;
            WorkerService = workerService;
        }
        #endregion

        #region Public class methods

        public void AddProcessor(TaskQueueComputationEntry computation)
        {
            Guid computationID = computation.ComputationID;

            var processor = new TaskQueueComputationProcessor(WorkerService, computation);
            bool isAdded = Processors.TryAdd(computationID, processor);
            if (isAdded)
            {
                processor.TaskAssigned += new EventHandler<TaskQueueTaskAssignedEventArgs>(ProcessorTaskAssigned);

                processor.Start();
            }
        }

        public void AcknowledgeTask(Guid workerID, Guid taskID)
        {
            WorkerService.ReleaseWorker(workerID);

            TaskQueueProcessedTaskEntry processedEntry = null;
            bool isRemoved = ProcessedTasks.TryRemove(taskID, out processedEntry);
            if (isRemoved)
            {
                processedEntry.StopTimer();

                int remainingTasksCount = 0, processedTasksCount = 0;
                TaskQueueTaskEntry taskEntry = processedEntry.TaskEntry;

                bool isFinished = FinishTask(taskEntry, out remainingTasksCount, out processedTasksCount);
                if (isFinished)
                {
                    NotificationService.TaskFinished(taskEntry.ClientID, taskEntry.ComputationID, taskEntry.TaskID, workerID);

                    bool isBarrierFinished = TryRemoveBarrierIfFinished(taskEntry.ComputationID, taskEntry.BarrierID);
                    if (isBarrierFinished)
                    {
                        NotificationService.BarrierFinished(taskEntry.ClientID, taskEntry.ComputationID, taskEntry.BarrierID);
                    }

                    if ((remainingTasksCount == 0) && (processedTasksCount == 0))
                    {
                        NotificationService.QueueEmptied(taskEntry.ClientID, taskEntry.ComputationID);
                    }
                }
            }
        }

        public void RemoveProcessor(Guid computationID)
        {
            TaskQueueComputationProcessor processor = null;
            bool isRemoved = Processors.TryRemove(computationID, out processor);
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
            NotificationService.TaskAssigned(taskEntry.ClientID, taskEntry.ComputationID, taskEntry.TaskID, workerID);

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
                
                Guid computationID = processedEntry.TaskEntry.ComputationID;
                TaskQueueComputationProcessor processor = null;
                bool isExists = Processors.TryGetValue(computationID, out processor);
                if (isExists)
                {
                    processor.EnqueueTask(processedEntry.TaskEntry);
                }
            }
        }

        private bool FinishTask(TaskQueueTaskEntry taskEntry, out int remainingTasksCount, out int processedTasksCount)
        {
            bool result = false;

            Guid computationID = taskEntry.ComputationID;

            TaskQueueComputationProcessor processor = null;
            bool isExists = Processors.TryGetValue(computationID, out processor);
            if (isExists)
            {
                result = processor.FinishTask(taskEntry);
            }

            remainingTasksCount = processor.TasksCount;
            processedTasksCount = processor.ProcessedTasksCount;

            return result;
        }

        private bool TryRemoveBarrierIfFinished(Guid computationID, Guid barrierID)
        {
            bool result = false;

            TaskQueueComputationProcessor processor = null;
            bool isExists = Processors.TryGetValue(computationID, out processor);
            if (isExists)
            {
                result = processor.TryRemoveBarrierIfFinished(barrierID);
            }

            return result;
        }
        #endregion
    }
}