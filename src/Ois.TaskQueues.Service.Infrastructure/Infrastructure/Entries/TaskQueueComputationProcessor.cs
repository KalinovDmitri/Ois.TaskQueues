using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueComputationProcessor
    {
        #region Constants and fields

        private const int ExecutionDelay = 100; // 100 milliseconds

        private readonly CancellationTokenSource TokenSource;

        private readonly TaskQueueWorkerService WorkerService;

        public readonly TaskQueueComputationEntry Computation;

        private Task ProcessingTask;
        #endregion

        #region Events

        public event EventHandler<TaskQueueTaskAssignedEventArgs> TaskAssigned;
        #endregion

        #region Properties

        public int TasksCount => Computation.TasksCount;
        #endregion

        #region Constructors

        private TaskQueueComputationProcessor()
        {
            TokenSource = new CancellationTokenSource();
        }

        public TaskQueueComputationProcessor(TaskQueueWorkerService workerService, TaskQueueComputationEntry computation) : this()
        {
            WorkerService = workerService;
            Computation = computation;
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

            ProcessingTask.Wait();
            ProcessingTask.Dispose();
        }

        public bool FinishTask(TaskQueueTaskEntry taskEntry)
        {
            return Computation.FinishTask(taskEntry);
        }

        public void EnqueueTask(TaskQueueTaskEntry taskEntry)
        {
            Computation.EnqueueTask(taskEntry);
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
                int tasksCount = Computation.TasksCount;

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

            bool taskDequeued = Computation.TryDequeueTask(out taskEntry);
            if (taskDequeued)
            {
                bool workerDequeued = WorkerService.TryDequeueWorker(taskEntry.TaskCategory, out workerEntry);
                if (workerDequeued)
                {
                    bool isAssigned = workerEntry.AssignTask(taskEntry);
                    if (isAssigned)
                    {
                        Computation.AddTaskToProcessed(taskEntry);
                        TaskAssigned?.Invoke(this, new TaskQueueTaskAssignedEventArgs(workerEntry.WorkerID, taskEntry));
                    }
                    else
                    {
                        Computation.EnqueueTask(taskEntry);
                        WorkerService.ReleaseWorker(workerEntry.WorkerID);
                    }
                }
                else
                {
                    Computation.EnqueueTask(taskEntry);
                }
            }
        }
        #endregion
    }
}