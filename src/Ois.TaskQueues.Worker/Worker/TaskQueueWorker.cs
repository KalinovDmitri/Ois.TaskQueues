using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;

using NLog;

namespace Ois.TaskQueues.Worker
{
    using Communication;
    using Infrastructure;

    public sealed class TaskQueueWorker : ITaskQueueWorkerCallback, IStartable, IDisposable
    {
        #region Constants and fields

        public static readonly Binding DefaultBinding;

        private const int LockTimeout = 1000; // 1 second

        private readonly object LockObject = new object();

        private readonly Guid WorkerID;

        private readonly TaskQueueWorkerConfiguration Configuration;

        private readonly ILogger Logger;

        private readonly Timer KeepAliveTimer;

        private readonly TaskQueueWorkerService Service;

        private bool IsRegistered;

        private Action<TaskQueueTaskInfo> ExecutionAction;
        #endregion

        #region Properties

        public TaskQueueWorkerImplementationService ImplementationService { get; set; }
        #endregion

        #region Constructors

        static TaskQueueWorker()
        {
            NetTcpBinding defaultBinding = new NetTcpBinding
            {
                OpenTimeout = TimeSpan.FromMinutes(2.0),
                ReceiveTimeout = TimeSpan.FromMinutes(5.0),
                SendTimeout = TimeSpan.FromMinutes(5.0),
                CloseTimeout = TimeSpan.FromMinutes(2.0),
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxArrayLength = int.MaxValue,
                    MaxBytesPerRead = int.MaxValue,
                    MaxDepth = 64,
                    MaxNameTableCharCount = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                },
                ReliableSession = new OptionalReliableSession
                {
                    Enabled = true,
                    InactivityTimeout = TimeSpan.FromMinutes(20.0),
                },
                Security = new NetTcpSecurity
                {
                    Mode = SecurityMode.None
                }
            };
            DefaultBinding = defaultBinding;
        }

        private TaskQueueWorker()
        {
            WorkerID = Guid.NewGuid();
            KeepAliveTimer = new Timer(WorkerKeepAlive, null, Timeout.Infinite, Timeout.Infinite);
        }

        public TaskQueueWorker(ILogger logger, TaskQueueWorkerConfiguration configuration) : this(logger, configuration, DefaultBinding) { }

        public TaskQueueWorker(ILogger logger, TaskQueueWorkerConfiguration configuration, Binding serviceBinding) : this()
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger), "Logger can't be null.");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Service configuration can't be null.");
            }
            if (serviceBinding == null)
            {
                throw new ArgumentNullException(nameof(serviceBinding), "Service binding can't be null.");
            }

            Configuration = configuration;
            Logger = logger;

            EndpointAddress endpoint = new EndpointAddress(configuration.ServiceEndpoint);
            InstanceContext callback = new InstanceContext(this);

            Service = new TaskQueueWorkerService(callback, serviceBinding, endpoint);
        }
        #endregion

        #region Interfaces implementation

        void IStartable.Start()
        {
            Logger.Debug("Worker registration request");

            Register();

            Logger.Debug($"Worker registration result = {IsRegistered}");

            TimeSpan keepAliveSpan = Configuration.KeepAlivePeriod;
            KeepAliveTimer.Change(keepAliveSpan, keepAliveSpan);
        }

        void IDisposable.Dispose()
        {
            Logger.Debug("Worker unregistration request");

            KeepAliveTimer.Dispose();
            Unregister();

            Logger.Debug($"Worker unregistration result = {!IsRegistered}");
        }

        public bool AssignTask(Guid taskID, string taskCategory, string taskData)
        {
            bool result = false;

            Logger.Debug($"AssignTask request; TaskID = {{{taskID}}}");

            if (IsRegistered)
            {
                bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
                if (isLocked)
                {
                    TaskQueueTaskInfo taskInfo = new TaskQueueTaskInfo(taskID, taskData);
                    result = AssignTaskCore(taskCategory, taskInfo);
                    Monitor.Exit(LockObject);
                }
                else
                {
                    Logger.Debug("AssignTask: lock not taken -> timeout exceeded");
                }
            }
            else
            {
                Logger.Debug("AssignTask: worker not registered");
            }

            return result;
        }
        #endregion

        #region Public class methods
        
        public void Register()
        {
            if (IsRegistered) return;

            bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
            if (isLocked)
            {
                IsRegistered = Service.RegisterWorker(WorkerID, Configuration.TaskCategories);
                Monitor.Exit(LockObject);
            }
        }

        public void Unregister()
        {
            if (IsRegistered)
            {
                bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
                if (isLocked)
                {
                    bool result = Service.UnregisterWorker(WorkerID);
                    if (result)
                    {
                        IsRegistered = false;
                    }
                    Monitor.Exit(LockObject);
                }
            }
        }
        #endregion

        #region Private class methods

        private bool AssignTaskCore(string taskCategory, TaskQueueTaskInfo taskInfo)
        {
            bool result = false;

            ITaskQueueWorkerImpl implementor = ImplementationService.ResolveImplementor(taskCategory);
            if (implementor != null)
            {
                Action<TaskQueueTaskInfo> executionAction = new Action<TaskQueueTaskInfo>(implementor.Execute);
                executionAction.BeginInvoke(taskInfo, ExecutionActionEndInvoke, taskInfo);
                ExecutionAction = executionAction;

                result = true;
            }
            else
            {
                Logger.Warn($"Implementor for category '{taskCategory}' not found!");
            }

            return result;
        }

        private void ExecutionActionEndInvoke(IAsyncResult result)
        {
            bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
            if (isLocked)
            {
                ExecutionAction?.EndInvoke(result);

                TaskQueueTaskInfo taskInfo = result.AsyncState as TaskQueueTaskInfo;
                if (taskInfo != null)
                {
                    Service.AcknowledgeTask(WorkerID, taskInfo.TaskID);
                    
                    Logger.Debug($"AcknowledgeTask: TaskID = {{{taskInfo.TaskID}}}");
                }
                Monitor.Exit(LockObject);
            }
            else
            {
                Logger.Debug("TaskExecutionEndInvoke: lock not taken -> timeout exceeded");
            }
        }

        private void WorkerKeepAlive(object state)
        {
            bool isLocked = Monitor.TryEnter(LockObject, LockTimeout);
            if (isLocked)
            {
                if (IsRegistered)
                {
                    Service.KeepAlive(WorkerID);
                }
                Monitor.Exit(LockObject);
            }
        }
        #endregion
    }
}