using System;

namespace Ois.TaskQueues.Worker
{
    public interface ITaskQueueWorkerImpl
    {

        void Execute(TaskQueueTaskInfo taskInfo);
    }
}