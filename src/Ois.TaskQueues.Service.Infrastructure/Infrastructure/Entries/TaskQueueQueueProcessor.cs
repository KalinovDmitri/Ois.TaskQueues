using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueQueueProcessor
    {
        #region Constants and fields

        private const int ExecutionDelay = 100; // 100 milliseconds

        private readonly CancellationTokenSource TokenSource;

        private readonly TaskQueueWorkerService WorkerService;

        public readonly TaskQueueQueueEntry ProcessedQueue;

        private Task ProcessingTask;
        #endregion

        #region Events

        public event EventHandler<TaskQueueTaskAssignedEventArgs> TaskAssigned;
        #endregion

        #region Properties

        public int TasksCount => ProcessedQueue.TasksCount;

        public int ProcessedTasksCount => ProcessedQueue.ProcessedTasksCount;
        #endregion

        #region Constructors

        private TaskQueueQueueProcessor()
        {
            TokenSource = new CancellationTokenSource();
        }

        public TaskQueueQueueProcessor(TaskQueueWorkerService workerService, TaskQueueQueueEntry queue) : this()
        {
            WorkerService = workerService;
            ProcessedQueue = queue;
        }
        #endregion

        #region Public class methods

        public void Start()
        {
            CancellationToken token = TokenSource.Token;

            ProcessingTask = Task.Run(new Action(ExecuteProcessing), token);
        }

        public void Stop()
        {
            TokenSource.Cancel();

            Task processingTask = ProcessingTask;
            if (processingTask != null)
            {
                processingTask.Wait();
                processingTask.Dispose();
            }
        }

        public bool FinishTask(TaskQueueTaskEntry taskEntry)
        {
            return ProcessedQueue.FinishTask(taskEntry);
        }

        public void EnqueueTask(TaskQueueTaskEntry taskEntry)
        {
            ProcessedQueue.EnqueueTask(taskEntry);
        }

        public bool TryRemoveBarrierIfFinished(Guid barrierID)
        {
            return ProcessedQueue.TryRemoveBarrierIfFinished(barrierID);
        }
        #endregion

        #region Private class methods

        private void ExecuteProcessing()
        {
            while (true)
            {
                bool isCanceled = TokenSource.IsCancellationRequested;
                if (isCanceled) break;

                int workersCount = WorkerService.AvailableWorkersCount;
                int tasksCount = ProcessedQueue.TasksCount;

                if (workersCount > 0 && tasksCount > 0)
                {
                    ExecuteProcessingCore();
                }
                else
                {
                    Thread.Sleep(ExecutionDelay);
                }
            }
        }

        private void ExecuteProcessingCore()
        {
            TaskQueueTaskEntry taskEntry = null;
            TaskQueueWorkerEntry workerEntry = null;

            bool taskDequeued = ProcessedQueue.TryDequeueTask(out taskEntry);
            if (taskDequeued)
            {
                bool workerDequeued = WorkerService.TryDequeueWorker(taskEntry.TaskCategory, out workerEntry);
                if (workerDequeued)
                {
                    bool isAssigned = workerEntry.AssignTask(taskEntry);
                    if (isAssigned)
                    {
                        ProcessedQueue.AddTaskToProcessed(taskEntry);
                        TaskAssigned?.Invoke(this, new TaskQueueTaskAssignedEventArgs(workerEntry.WorkerID, taskEntry));
                    }
                    else
                    {
                        ProcessedQueue.EnqueueTask(taskEntry);
                        WorkerService.ReleaseWorker(workerEntry.WorkerID);
                    }
                }
                else
                {
                    ProcessedQueue.EnqueueTask(taskEntry);
                }
            }
        }
        #endregion
    }
}