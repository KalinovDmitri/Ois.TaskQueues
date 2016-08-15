using System;

namespace Ois.TaskQueues.Worker
{
    /// <summary>
    /// Предоставляет метаданные реализатора
    /// </summary>
    public interface ITaskQueueWorkerMetadata
    {
        /// <summary>
        /// Возвращает или задаёт название категории задач, обрабатываемых реализатором
        /// </summary>
        string TaskCategory { get; }
    }
}