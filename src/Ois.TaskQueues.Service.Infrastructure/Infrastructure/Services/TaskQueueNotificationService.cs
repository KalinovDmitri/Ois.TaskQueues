using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NLog;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public class TaskQueueNotificationService : IStartable, IDisposable
    {
        #region Constants and fields

        private const int ExecutionDelay = 100;

        private readonly JsonConverter[] Converters;

        private readonly CancellationTokenSource TokenSource;

        private readonly ILogger Logger;

        private Task ExecutionTask;

        private int ClientsCount = 0;

        private int EventsCount = 0;

        private ConcurrentDictionary<Guid, TaskQueueClientEntry> RegisteredClients;

        private ConcurrentQueue<TaskQueueEventInfo> EventsQueue;
        #endregion

        #region Constructors

        private TaskQueueNotificationService()
        {
            Converters = new JsonConverter[]
            {
                new StringEnumConverter()
            };

            TokenSource = new CancellationTokenSource();
            
            EventsQueue = new ConcurrentQueue<TaskQueueEventInfo>();
            RegisteredClients = new ConcurrentDictionary<Guid, TaskQueueClientEntry>();
        }

        public TaskQueueNotificationService(ILogger logger) : this()
        {
            Logger = logger;
        }
        #endregion

        #region Public class methods

        public void Start()
        {
            CancellationToken token = TokenSource.Token;

            ExecutionTask = Task.Run(new Action(ExecuteNotification), token);
        }

        public void Dispose()
        {
            TokenSource.Cancel();

            Task executionTask = ExecutionTask;
            if (executionTask != null)
            {
                executionTask.Wait();
                executionTask.Dispose();
            }
        }
        #endregion

        #region Public class methods

        public void ClientConnected(TaskQueueClientEntry clientEntry)
        {
            bool isAdded = RegisteredClients.TryAdd(clientEntry.ClientID, clientEntry);
            if (isAdded)
            {
                Interlocked.Increment(ref ClientsCount);

                JObject eventData = new JObject();

                eventData.Add("ClientID", new JValue(clientEntry.ClientID));

                EnqueueEvent(clientEntry.ClientID, TaskQueueServiceEvents.ClientConnected, eventData);
            }
        }

        public void ClientDisconnected(Guid clientID)
        {
            TaskQueueClientEntry clientEntry = null;
            bool isRemoved = RegisteredClients.TryRemove(clientID, out clientEntry);
            if (isRemoved)
            {
                Interlocked.Decrement(ref ClientsCount);

                JObject eventData = new JObject();

                eventData.Add("ClientID", new JValue(clientID));

                EnqueueEvent(Guid.Empty, TaskQueueServiceEvents.ClientDisconnected, eventData);
            }
        }

        public void ComputationCreated(Guid clientID, Guid computationID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.ComputationCreated, eventData);
        }

        public void ComputationFinished(Guid clientID, Guid computationID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.ComputationFinished, eventData);
        }

        public void TaskAdded(Guid clientID, Guid computationID, Guid taskID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));
            eventData.Add("TaskID", new JValue(taskID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.TaskCreated, eventData);
        }

        public void TaskAssigned(Guid clientID, Guid computationID, Guid taskID, Guid workerID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));
            eventData.Add("TaskID", new JValue(taskID));
            eventData.Add("WorkerID", new JValue(workerID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.TaskAssigned, eventData);
        }

        public void TaskFinished(Guid clientID, Guid computationID, Guid taskID, Guid workerID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));
            eventData.Add("TaskID", new JValue(taskID));
            eventData.Add("WorkerID", new JValue(workerID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.TaskFinished, eventData);
        }

        public void BarrierAdded(Guid clientID, Guid computationID, Guid barrierID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));
            eventData.Add("BarrierID", new JValue(barrierID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.BarrierCreated, eventData);
        }

        public void BarrierFinished(Guid clientID, Guid computationID, Guid barrierID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));
            eventData.Add("BarrierID", new JValue(barrierID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.BarrierFinished, eventData);
        }

        public void QueueEmptied(Guid clientID, Guid computationID)
        {
            JObject eventData = new JObject();

            eventData.Add("ClientID", new JValue(clientID));
            eventData.Add("ComputationID", new JValue(computationID));

            EnqueueEvent(clientID, TaskQueueServiceEvents.QueueEmptied, eventData);
        }

        public void WorkerConnected(Guid workerID, string[] taskCategories)
        {
            JObject eventData = new JObject();

            eventData.Add("WorkerID", new JValue(workerID));
            eventData.Add("TaskCategories", new JArray(taskCategories));

            EnqueueEvent(Guid.Empty, TaskQueueServiceEvents.WorkerConnected, eventData);
        }

        public void WorkerDisconnected(Guid workerID)
        {
            JObject eventData = new JObject();

            eventData.Add("WorkerID", new JValue(workerID));

            EnqueueEvent(Guid.Empty, TaskQueueServiceEvents.WorkerDisconnected, eventData);
        }
        #endregion

        #region Private class methods

        private void EnqueueEvent(Guid clientID, TaskQueueServiceEvents eventType, JObject jsonEventData)
        {
            string eventData = jsonEventData.ToString(Formatting.None, Converters);
            var eventInfo = new TaskQueueEventInfo(clientID, eventType, eventData);

            EventsQueue.Enqueue(eventInfo);
            Interlocked.Increment(ref EventsCount);

            Logger.Debug($"Event {eventType} enqueued; event data: {eventData}");
        }

        private bool TryDequeueEvent(out TaskQueueEventInfo eventInfo)
        {
            bool result = EventsQueue.TryDequeue(out eventInfo);
            if (result)
            {
                Interlocked.Decrement(ref EventsCount);
            }
            return result;
        }

        private void ExecuteNotification()
        {
            while (true)
            {
                bool isCanceled = TokenSource.IsCancellationRequested;
                if (isCanceled) break;

                if ((ClientsCount > 0) && (EventsCount > 0))
                {
                    ExecuteNotificationCore();
                }
                else
                {
                    Thread.Sleep(ExecutionDelay);
                }
            }
        }

        private void ExecuteNotificationCore()
        {
            TaskQueueEventInfo eventInfo = null;
            bool eventExists = TryDequeueEvent(out eventInfo);
            if (eventExists)
            {
                foreach (KeyValuePair<Guid, TaskQueueClientEntry> clientPair in RegisteredClients)
                {
                    TaskQueueClientEntry clientEntry = clientPair.Value;

                    if (clientEntry.IsDisposed) continue;

                    bool isSubscribed = ((clientEntry.SubscribedEvents & eventInfo.EventType) != TaskQueueServiceEvents.None);
                    if (isSubscribed)
                    {
                        if ((eventInfo.ClientID == Guid.Empty) || !clientEntry.OnlyOwnEvents) // event for everyone OR client subscribed to all events
                        {
                            clientEntry.EventOccured(eventInfo);
                        }
                        else if (clientEntry.OnlyOwnEvents && (clientEntry.ClientID == eventInfo.ClientID))
                        {
                            clientEntry.EventOccured(eventInfo);
                        }
                    }
                }
            }
        }
        #endregion
    }
}