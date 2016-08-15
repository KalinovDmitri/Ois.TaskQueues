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
                computation.BarrierFinished += new EventHandler<TaskQueueBarrierEntry>(ComputationBarrierFinished);
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

                TaskQueueTaskEntry taskEntry = processedEntry.TaskEntry;
                bool isFinished = FinishTask(taskEntry);
                if (isFinished)
                {
                    NotificationService.TaskFinished(taskEntry.ClientID, taskEntry.ComputationID, taskEntry.TaskID, workerID);
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

        private void ComputationBarrierFinished(object sender, TaskQueueBarrierEntry barrierEntry)
        {
            NotificationService.BarrierFinished(barrierEntry.ClientID, barrierEntry.ComputationID, barrierEntry.BarrierID);
        }

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

        private bool FinishTask(TaskQueueTaskEntry taskEntry)
        {
            bool result = false;

            Guid computationID = taskEntry.ComputationID;

            TaskQueueComputationProcessor processor = null;
            bool isExists = Processors.TryGetValue(computationID, out processor);
            if (isExists)
            {
                result = processor.FinishTask(taskEntry);
            }

            if (processor.TasksCount == 0)
            {
                NotificationService.QueueEmptied(processor.Computation.ClientID, computationID);
            }

            return result;
        }
        #endregion
    }
}