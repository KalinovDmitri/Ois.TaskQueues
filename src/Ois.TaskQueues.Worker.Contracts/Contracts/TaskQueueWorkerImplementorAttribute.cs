using System;
using System.ComponentModel.Composition;

namespace Ois.TaskQueues.Worker
{
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TaskQueueWorkerImplementorAttribute : ExportAttribute
    {
        #region Properties

        public string TaskCategory { get; private set; }
        #endregion

        #region Constructors

        public TaskQueueWorkerImplementorAttribute(string taskCategory) : base(typeof(ITaskQueueWorkerImpl))
        {
            TaskCategory = taskCategory;
        }
        #endregion
    }
}