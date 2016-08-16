using System;

namespace Ois.TaskQueues.Worker
{
    /// <summary>
    /// Определяет аспекты поведения внешнего реализатора воркера
    /// </summary>
    public interface ITaskQueueWorkerImpl
    {
        /// <summary>
        /// Выполняет обработку задачи, представленной экземпляром класса <see cref="TaskQueueTaskInfo"/>
        /// </summary>
        /// <param name="taskInfo">Данные обрабатываемой задачи</param>
        void Execute(TaskQueueTaskInfo taskInfo);
    }
}