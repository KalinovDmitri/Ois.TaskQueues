using System;
using System.ServiceModel;

namespace Ois.TaskQueues.Service.Infrastructure
{
    using Communication;

    internal class TaskQueueWorker : ITaskQueueWorker, IDisposable
    {
        #region Fields

        private ITaskQueueWorkerCallback Callback;
        #endregion

        #region Constructors

        public TaskQueueWorker(ITaskQueueWorkerCallback callback)
        {
            Callback = callback;
        }
        #endregion

        #region Public class methods

        public bool AssignTask(Guid taskID, string taskCategory, string taskData)
        {
            bool result = Callback.AssignTask(taskID, taskCategory, taskData);
            return result;
        }

        public void Dispose()
        {
            ICommunicationObject channel = Callback as ICommunicationObject;
            if (channel != null)
            {
                channel.Close();
            }
        }
        #endregion
    }
}