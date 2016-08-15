using System;

namespace Ois.TaskQueues
{
    /// <summary>
    /// Предоставляет доступ к параметрам конфигурации сервиса очередей задач. Данный класс не может наследоваться.
    /// </summary>
    public sealed class TaskQueueServiceConfiguration
    {
        #region Static members

        private static TaskQueueServiceConfiguration InnerInstance;

        /// <summary>
        /// Возвращает текущий активный экземпляр конфигурации
        /// </summary>
        public static TaskQueueServiceConfiguration Instance => InnerInstance;

        public static void SetInstance(TaskQueueServiceConfiguration instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "The instance of TaskQueue service configuration can't be null.");
            }

            InnerInstance = instance;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Возвращает или задаёт интервал выполнения отдельной задачи. Значение по умолчанию - 30 секунд.
        /// </summary>
        public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30.0);
        /// <summary>
        /// Возвращает или задаёт имя хоста для регистрации сервисов. Значение по умолчанию - <see cref="Environment.MachineName"/>.
        /// </summary>
        public string ServiceHost { get; set; } = Environment.MachineName;
        /// <summary>
        /// Возвращает или задаёт порт для подключения клиентов. Значение по умолчанию - 8788.
        /// </summary>
        public int ClientServicePort { get; set; } = 8788;
        /// <summary>
        /// Возвращает или задаёт порт для подключения воркеров. Значение по умолчанию - 8789.
        /// </summary>
        public int WorkerServicePort { get; set; } = 8789;
        /// <summary>
        /// Возвращает или задаёт начальное количество запускаемых воркеров в случае их отсутствия. Значение по умолчанию - 1.
        /// </summary>
        public int WorkersInitialCount { get; set; } = 1;
        /// <summary>
        /// Возвращает или задаёт путь к файлу для запуска нового воркера
        /// </summary>
        public string WorkersRunFile { get; set; } = "Balancing\\RunWorker.cmd";
        /// <summary>
        /// Возвращает или задаёт путь к файлу для останова запущенного воркера
        /// </summary>
        public string WorkersStopFile { get; set; } = "Balancing\\StopWorker.cmd";
        /// <summary>
        /// Возвращает или задаёт время ожидания перед запуском нового воркера. Значение по умолчанию - 5 секунд.
        /// </summary>
        public TimeSpan RunTimerDueTime { get; set; } = TimeSpan.FromSeconds(5.0);
        /// <summary>
        /// Возвращает или задаёт интервал между запусками новых воркеров. Значение по умолчанию - 20 секунд.
        /// </summary>
        public TimeSpan RunTimerInterval { get; set; } = TimeSpan.FromSeconds(20.0);
        /// <summary>
        /// Возвращает или задаёт время ожидания перед остановом воркера. Значение по умолчанию - 20 секунд.
        /// </summary>
        public TimeSpan StopTimerDueTime { get; set; } = TimeSpan.FromSeconds(20.0);
        /// <summary>
        /// Возвращает или задаёт интервал между остановами воркеров. Значение по умолчанию - 20 секунд.
        /// </summary>
        public TimeSpan StopTimerInterval { get; set; } = TimeSpan.FromSeconds(20.0);
        #endregion

        #region Constructors

        static TaskQueueServiceConfiguration()
        {
            InnerInstance = new TaskQueueServiceConfiguration();
        }

        internal TaskQueueServiceConfiguration() { }
        #endregion
    }
}