using System;

namespace Ois.TaskQueues.Worker
{
    /// <summary>
    /// Предоставляет доступ к параметрам конфигурации воркера очередей задач. Данный класс не может наследоваться.
    /// </summary>
    public sealed class TaskQueueWorkerConfiguration
    {
        #region Static members

        private static TaskQueueWorkerConfiguration InnerInstance;

        /// <summary>
        /// Возвращает текущий активный экземпляр конфигурации
        /// </summary>
        public static TaskQueueWorkerConfiguration Instance => InnerInstance;

        public static void SetInstance(TaskQueueWorkerConfiguration instance)
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
        /// Возвращает или задаёт адрес сервиса для подключения.
        /// </summary>
        public string ServiceEndpoint { get; set; } = "net.tcp://DKalinov-PC:8789/tqservice";
        /// <summary>
        /// Возвращает или задаёт интервал отправки keep-alive сообщения. Значение по умолчанию - 10 секунд.
        /// </summary>
        public TimeSpan KeepAlivePeriod { get; set; } = TimeSpan.FromSeconds(10.0);
        /// <summary>
        /// Возвращает или задаёт название имплементора, выполняющего обработку задачи.
        /// </summary>
        public string ImplementorName { get; set; } = "SimpleImplementor";
        /// <summary>
        /// Возвращает или задаёт категории задач, которые способен обрабатывать воркер.
        /// </summary>
        public string[] TaskCategories { get; set; } = { "Unknown" };
        #endregion

        #region Constructors

        static TaskQueueWorkerConfiguration()
        {
            InnerInstance = new TaskQueueWorkerConfiguration();
        }

        internal TaskQueueWorkerConfiguration() { }
        #endregion
    }
}