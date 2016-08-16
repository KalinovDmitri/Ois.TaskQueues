using System;
using System.ComponentModel.Composition;

namespace Ois.TaskQueues.Worker
{
    /// <summary>
    /// Указывает, что целевой класс является внешним реализатором воркера
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TaskQueueWorkerImplementorAttribute : ExportAttribute
    {
        #region Properties
        /// <summary>
        /// Возвращает или задаёт категорию задач, обрабатываемых реализатором
        /// </summary>
        public string TaskCategory { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TaskQueueWorkerImplementorAttribute"/>
        /// </summary>
        /// <param name="taskCategory">Название категории задач, обрабатываемых реализатором</param>
        public TaskQueueWorkerImplementorAttribute(string taskCategory) : base(typeof(ITaskQueueWorkerImpl))
        {
            TaskCategory = taskCategory;
        }
        #endregion
    }
}