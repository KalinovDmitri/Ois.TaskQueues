using System;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Ois.TaskQueues.Worker
{
    [TaskQueueWorkerImplementor("TestCategory")]
    public sealed class SimpleWorkerImplementor : ITaskQueueWorkerImpl
    {
        #region Constants and fields

        private const int ExecutionDelay = 5000; // 5000 milliseconds <==> 5 seconds

        private readonly ILogger Logger;
        #endregion

        #region Constructors

        public SimpleWorkerImplementor()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }
        #endregion

        #region ITaskQueueWorkerImpl implementation

        public void Execute(TaskQueueTaskInfo taskInfo)
        {
            JObject jsonData = JObject.Parse(taskInfo.TaskData);
            if (jsonData != null)
            {
                Thread.Sleep(ExecutionDelay);

                string taskData = jsonData.ToString(Formatting.Indented);
                Logger.Debug(taskData);
            }
        }
        #endregion
    }
}