﻿using System;

using NLog;

namespace Ois.TaskQueues.Service.Infrastructure
{
    internal class TaskQueueClientMock : ITaskQueueClient
    {
        #region Fields and properties

        private readonly ILogger Logger;

        public bool OnlyOwnEvents => false;

        public TaskQueueServiceEvents SubscribedEvents => TaskQueueServiceEvents.All;
        #endregion

        #region Constructors

        public TaskQueueClientMock(ILogger logger)
        {
            Logger = logger;
        }
        #endregion

        #region Public class methods

        public void EventOccured(TaskQueueServiceEvents occuredEvent, string eventData)
        {
            string message = $"Event occured:\r\ntype = {occuredEvent}\r\ndata = {eventData}";

            Logger.Debug(message);
        }

        public void Dispose() { }
        #endregion
    }
}