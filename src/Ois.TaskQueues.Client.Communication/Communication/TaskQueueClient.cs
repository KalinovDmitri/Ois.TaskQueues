using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Ois.TaskQueues.Communication
{
    [CallbackBehavior(AutomaticSessionShutdown = false)]
    public sealed class TaskQueueClient : ITaskQueueClientCallback, IDisposable
    {
        #region Constants and fields

        private const string ClientNotRegisteredMessage = "The TaskQueue client can't be used before registration.";

        private const string ClientAlreadyRegisteredMessage = "The TaskQueue client already registered.";

        private const string ClientDisposedMessage = "The TaskQueue client can't be used after disposing.";

        public static readonly Binding DefaultBinding;

        public readonly Guid ClientID;

        private bool IsRegistered;

        private bool IsDisposed;

        private readonly TaskQueueClientService Service;
        #endregion

        #region Events

        public event EventHandler<TaskQueueEventOccuredEventArgs> EventOccured;
        #endregion

        #region Constructors

        static TaskQueueClient()
        {
            NetTcpBinding serviceBinding = new NetTcpBinding
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
                    InactivityTimeout = TimeSpan.FromMinutes(20.0)
                },
                Security = new NetTcpSecurity
                {
                    Mode = SecurityMode.None
                }
            };
            DefaultBinding = serviceBinding;
        }

        private TaskQueueClient()
        {
            ClientID = Guid.NewGuid();

            IsRegistered = false;
            IsDisposed = false;
        }

        public TaskQueueClient(string serviceAddress) : this(serviceAddress, DefaultBinding) { }

        public TaskQueueClient(string serviceAddress, Binding serviceBinding) : this()
        {
            if (string.IsNullOrEmpty(serviceAddress))
            {
                throw new ArgumentNullException(nameof(serviceAddress), "Service address can't be null or empty.");
            }
            if (serviceBinding == null)
            {
                throw new ArgumentNullException(nameof(serviceAddress), "Service binding can't be null.");
            }

            EndpointAddress endpoint = new EndpointAddress(serviceAddress);
            InstanceContext callback = new InstanceContext(this);

            Service = new TaskQueueClientService(callback, serviceBinding, endpoint);
        }
        #endregion

        #region Public class methods

        public bool Register(bool onlyOwnEvents = true, TaskQueueServiceEvents subscribedEvents = TaskQueueServiceEvents.All)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException(ClientDisposedMessage);
            }
            if (IsRegistered)
            {
                throw new InvalidOperationException(ClientAlreadyRegisteredMessage);
            }

            bool result = Service.RegisterClient(ClientID, onlyOwnEvents, subscribedEvents);
            IsRegistered = result;
            return result;
        }

        public Guid CreateQueue()
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException(ClientDisposedMessage);
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException(ClientNotRegisteredMessage);
            }

            Guid queueID = Service.CreateQueue(ClientID);
            return queueID;
        }

        public Guid CreateTask(Guid queueID, string taskCategory, string taskData)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException(ClientDisposedMessage);
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException(ClientNotRegisteredMessage);
            }

            Guid taskID = Service.AddTask(queueID, taskCategory, taskData);
            return taskID;
        }

        public Guid CreateBarrier(Guid queueID)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException(ClientDisposedMessage);
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException(ClientNotRegisteredMessage);
            }

            Guid barrierID = Service.AddBarrier(queueID);
            return barrierID;
        }

        public void RemoveQueue(Guid queueID)
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException(ClientDisposedMessage);
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException(ClientNotRegisteredMessage);
            }

            Service.RemoveQueue(queueID);
        }

        public bool Unregister()
        {
            if (IsDisposed)
            {
                throw new InvalidOperationException(ClientDisposedMessage);
            }

            bool result = Service.UnregisterClient(ClientID);
            if (result)
            {
                IsRegistered = false;
            }
            return result;
        }
        #endregion

        #region Interfaces implementation

        void ITaskQueueClientCallback.EventOccured(TaskQueueServiceEvents occuredEvent, string eventData)
        {
            EventOccured?.Invoke(this, new TaskQueueEventOccuredEventArgs(occuredEvent, eventData));
        }

        public void Dispose()
        {
            bool result = Unregister();
            if (result)
            {
                Service.Close();
                IsDisposed = true;
            }
        }
        #endregion
    }
}