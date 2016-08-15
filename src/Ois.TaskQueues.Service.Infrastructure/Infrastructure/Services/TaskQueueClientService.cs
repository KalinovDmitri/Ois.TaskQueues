using System;
using System.Collections.Concurrent;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public class TaskQueueClientService
    {
        #region Constants and fields

        private ConcurrentDictionary<Guid, TaskQueueClientEntry> Clients;
        #endregion

        #region Events

        public event EventHandler<TaskQueueClientEntry> ClientRegistered;

        public event EventHandler<TaskQueueClientEntry> ClientUnregistered;
        #endregion

        #region Properties

        public TaskQueueNotificationService NotificationService { get; set; }
        #endregion

        #region Constructors

        public TaskQueueClientService()
        {
            Clients = new ConcurrentDictionary<Guid, TaskQueueClientEntry>();
        }
        #endregion

        #region Public class methods

        public TaskQueueClientEntry RegisterClient(Guid clientID, ITaskQueueClient client)
        {
            TaskQueueClientEntry clientEntry = new TaskQueueClientEntry(clientID, client);

            bool isAdded = Clients.TryAdd(clientID, clientEntry);

            return (isAdded) ? clientEntry : TaskQueueClientEntry.Empty;
        }

        public bool TryGetClient(Guid clientID, out TaskQueueClientEntry clientEntry)
        {
            return Clients.TryGetValue(clientID, out clientEntry);
        }

        public bool UnregisterClient(Guid clientID)
        {
            TaskQueueClientEntry clientEntry = null;
            bool isRemoved = Clients.TryRemove(clientID, out clientEntry);
            if (isRemoved)
            {
                clientEntry.Dispose();
            }
            return isRemoved;
        }
        #endregion

        #region Private class methods

        private void OnClientRegistered(TaskQueueClientEntry clientEntry)
        {
            ClientRegistered?.Invoke(this, clientEntry);
        }

        private void OnClientUnregistered(TaskQueueClientEntry clientEntry)
        {
            ClientUnregistered?.Invoke(this, clientEntry);
        }
        #endregion
    }
}