using System;

namespace Ois.TaskQueues.Service.Infrastructure
{
    /// <summary>
    /// Содержит перечислимые константы, определяющие тип элемента в очереди
    /// </summary>
    public enum TaskQueueItemType
    {
        /// <summary>
        /// Элемент является задачей
        /// </summary>
        Task = 1,
        /// <summary>
        /// Элемент является барьером
        /// </summary>
        Barrier = 2
    }
}