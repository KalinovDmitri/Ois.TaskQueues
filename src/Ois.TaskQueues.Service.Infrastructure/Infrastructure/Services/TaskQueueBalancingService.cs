using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NLog;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public sealed class TaskQueueBalancingService : IStartable, IDisposable
    {
        #region Fields and constants

        private const int ExecutionDelay = 100; // 100 milliseconds

        private const int RunningStoppingDelay = 1000; // 1 second

        private const string WorkerRunFileNotExists = "";

        private const string WorkerStopFileNotExists = "";

        private readonly ILogger Logger;

        private readonly TaskQueueServiceConfiguration Configuration;

        private bool Initialized = false;

        private string RunFilePath;

        private string StopFilePath;

        private int TasksCount = 0;

        private int WorkersCount = 0;

        private CancellationTokenSource CancellationSource;

        private Task ExecutionTask;
        #endregion

        #region Constructors

        private TaskQueueBalancingService()
        {
            CancellationSource = new CancellationTokenSource();
        }

        public TaskQueueBalancingService(ILogger logger, TaskQueueServiceConfiguration configuration) : this()
        {
            Logger = logger;
            Configuration = configuration;

            Initialize();
        }
        #endregion

        #region Interfaces implementation

        public void Start()
        {
            if (Initialized)
            {
                CancellationToken token = CancellationSource.Token;

                ExecutionTask = Task.Run(new Action(ExecuteProcessing), token);
            }
        }

        public void Dispose()
        {
            CancellationSource.Cancel();

            Task executionTask = ExecutionTask;
            if (executionTask != null)
            {
                executionTask.Wait();
                executionTask.Dispose();
            }
        }
        #endregion

        #region Public class methods

        public void TaskAdded()
        {
            Interlocked.Increment(ref TasksCount);
        }

        public void TaskFinished()
        {
            Interlocked.Decrement(ref TasksCount);
        }

        public void WorkerConnected()
        {
            Interlocked.Increment(ref WorkersCount);
        }

        public void WorkerDisconnected()
        {
            Interlocked.Decrement(ref WorkersCount);
        }
        #endregion

        #region Private class methods

        private void Initialize()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            string runFilePath = Path.Combine(currentDirectory, Configuration.WorkersRunFile);
            string stopFilePath = Path.Combine(currentDirectory, Configuration.WorkersStopFile);

            bool runFileExists = File.Exists(runFilePath);
            if (runFileExists)
            {
                RunFilePath = runFilePath;
            }
            else
            {
                Logger.Warn(WorkerRunFileNotExists);
            }

            bool stopFileExists = File.Exists(stopFilePath);
            if (stopFileExists)
            {
                StopFilePath = stopFilePath;
            }
            else
            {
                Logger.Warn(WorkerStopFileNotExists);
            }

            Initialized = runFileExists && stopFileExists;
        }

        private void ExecuteProcessing()
        {
            while (true)
            {
                bool isCanceled = CancellationSource.IsCancellationRequested;
                if (isCanceled) break;

                if (TasksCount > 0)
                {
                    if (WorkersCount <= Configuration.WorkersMinCount)
                    {
                        StartWorkers();
                    }
                }
                else
                {
                    if (WorkersCount > Configuration.WorkersMinCount)
                    {
                        StopWorkers();
                    }
                    else
                    {
                        Thread.Sleep(ExecutionDelay);
                    }
                }
            }
        }

        private void StartWorkers()
        {
            int currentCount = WorkersCount;
            int maxCount = Configuration.WorkersMaxCount;

            while (currentCount < maxCount)
            {
                StartWorker();
                ++currentCount;
            }
        }

        private void StopWorkers()
        {
            int minCount = Configuration.WorkersMinCount;
            int currentCount = WorkersCount;

            while (currentCount > minCount)
            {
                StopWorker();
                --currentCount;
            }
        }

        private void StartWorker()
        {
            ProcessStartInfo startInfo = GetStartInfo(true);
            Process.Start(startInfo).WaitForExit();
            Thread.Sleep(RunningStoppingDelay);
        }

        private void StopWorker()
        {
            ProcessStartInfo startInfo = GetStartInfo(false);
            Process.Start(startInfo).WaitForExit();
            Thread.Sleep(RunningStoppingDelay);
        }

        private ProcessStartInfo GetStartInfo(bool forRunning = true)
        {
            string filePath = (forRunning) ? RunFilePath : StopFilePath;

            ProcessStartInfo startInfo = new ProcessStartInfo(filePath)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            return startInfo;
        }
        #endregion
    }
}