using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    public interface ITaskQueueWorker : IDisposable
    {

        bool AssignTask(Guid taskID, string taskCategory, string taskData);
    }
}