using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ois.TaskQueues.Client.Interactivity
{
    using Communication;

    internal class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region Constants and fields

        private const string ServiceAddress = "net.tcp://DKalinov-PC:8788/tqservice";

        private const string TestTaskCategory = "TestCategory";

        private const string TestTaskData = "{ \"SomePropertyName\": \"SomePropertyValue\" }";

        private const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss.fff";

        private const bool OnlyOwnEvents = false;

        private const TaskQueueServiceEvents SubscribedEvents = TaskQueueServiceEvents.All;

        private TaskQueueClient Client;

        private ObservableCollection<OccuredEventViewModel> EventsCollection;

        private bool ClientConnectedValue = false;

        private int TasksCountValue = 5;

        private bool UseTaskTemplateValue = true;

        private Guid QueueID = Guid.Empty;

        private Window SettingsEditor;
        #endregion

        #region Properties

        public bool ClientConnected
        {
            get { return ClientConnectedValue; }
            private set
            {
                ClientConnectedValue = value;
                RaisePropertyChanged();
            }
        }

        public Guid CurrentQueueID
        {
            get { return QueueID; }
            private set
            {
                QueueID = value;
                RaisePropertyChanged();
            }
        }

        public int TasksCount
        {
            get { return TasksCountValue; }
            set
            {
                TasksCountValue = value;
                RaisePropertyChanged();
            }
        }

        public bool UseTaskTemplate
        {
            get { return UseTaskTemplateValue; }
            set
            {
                UseTaskTemplateValue = value;
                RaisePropertyChanged();
            }
        }
        
        public ObservableCollection<OccuredEventViewModel> OccuredEvents
        {
            get { return EventsCollection; }
        }
        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler EventsCollectionChanged
        {
            add
            {
                EventsCollection.CollectionChanged += value;
            }
            remove
            {
                EventsCollection.CollectionChanged -= value;
            }
        }
        #endregion

        #region Commands

        private DelegateCommand CommandOpenSettings;
        private DelegateCommand CommandRegisterClient;
        private DelegateCommand CommandUnregisterClient;
        private DelegateCommand CommandCreateQueue;
        private DelegateCommand CommandCreateTask;
        private DelegateCommand CommandCreateTaskGroup;
        private DelegateCommand CommandCreateBarrier;
        private DelegateCommand CommandRemoveQueue;

        public ICommand OpenSettingsCommand
        {
            get
            {
                return CommandOpenSettings ?? (CommandOpenSettings = new DelegateCommand(OpenSettings));
            }
        }

        public ICommand RegisterClientCommand
        {
            get
            {
                return CommandRegisterClient ?? (CommandRegisterClient = new DelegateCommand(RegisterClient, CanRegisterClient));
            }
        }

        public ICommand UnregisterClientCommand
        {
            get
            {
                return CommandUnregisterClient ?? (CommandUnregisterClient = new DelegateCommand(UnregisterClient, CanUnregisterClient));
            }
        }

        public ICommand CreateQueueCommand
        {
            get
            {
                return CommandCreateQueue ?? (CommandCreateQueue = new DelegateCommand(CreateQueue, CanCreateQueue));
            }
        }

        public ICommand CreateTaskCommand
        {
            get
            {
                return CommandCreateTask ?? (CommandCreateTask = new DelegateCommand(CreateTask, CanCreateTask));
            }
        }

        public ICommand CreateTaskGroupCommand
        {
            get
            {
                return CommandCreateTaskGroup ?? (CommandCreateTaskGroup = new DelegateCommand(CreateTaskGroup, CanCreateTask));
            }
        }

        public ICommand CreateBarrierCommand
        {
            get
            {
                return CommandCreateBarrier ?? (CommandCreateBarrier = new DelegateCommand(CreateBarrier, CanCreateBarrier));
            }
        }

        public ICommand RemoveQueueCommand
        {
            get
            {
                return CommandRemoveQueue ?? (CommandRemoveQueue = new DelegateCommand(RemoveQueue, CanRemoveQueue));
            }
        }
        #endregion

        #region Constructors

        internal MainWindowViewModel() : base()
        {
            EventsCollection = new ObservableCollection<OccuredEventViewModel>();

            TaskQueueClient client = new TaskQueueClient(ServiceAddress);
            client.EventOccured += new EventHandler<TaskQueueEventOccuredEventArgs>(ClientEventOccured);

            Client = client;
        }
        #endregion

        #region Public class methods

        public void Dispose()
        {
            try
            {
                Guid queueID = QueueID;
                if (queueID != Guid.Empty)
                {
                    Client.RemoveQueue(queueID);
                }

                Client.Dispose();
            }
            catch { }
            finally
            {
                Client = null;
            }
        }
        #endregion

        #region Command methods

        private void OpenSettings()
        {
            Window settingsEditor = new SettingsWindow()
            {
                DataContext = this
            };
            SettingsEditor = settingsEditor;
            settingsEditor.ShowDialog();
        }

        private bool CanRegisterClient()
        {
            return (Client != null) && !ClientConnectedValue;
        }

        private void RegisterClient()
        {
            ClientConnected = Client.Register(OnlyOwnEvents, SubscribedEvents);
        }

        private bool CanUnregisterClient()
        {
            return (Client != null) && ClientConnectedValue;
        }

        private void UnregisterClient()
        {
            Guid queueID = QueueID;
            if (queueID != Guid.Empty)
            {
                Client.RemoveQueue(queueID);
            }

            bool result = Client.Unregister();
            if (result)
            {
                ClientConnected = false;
            }
        }

        private bool CanCreateQueue()
        {
            return (Client != null) && ClientConnectedValue;
        }

        private void CreateQueue()
        {
            CurrentQueueID = Client.CreateQueue();
        }

        private bool CanCreateTask()
        {
            return (Client != null) && (QueueID != Guid.Empty);
        }

        private void CreateTask()
        {
            Guid taskID = Guid.Empty;

            if (UseTaskTemplateValue)
            {
                taskID = Client.CreateTask(QueueID, TestTaskCategory, TestTaskData);
                return;
            }

            string[] taskInfo = TaskEditorWindow.Show();

            string taskCategory = taskInfo[0];
            string taskData = taskInfo[1];

            taskID = Client.CreateTask(QueueID, taskCategory, taskData);
        }

        private void CreateTaskGroup()
        {
            int tasksCount = TasksCountValue;

            for (int index = 0; index < tasksCount; ++index)
            {
                Guid taskID = Client.CreateTask(QueueID, TestTaskCategory, TestTaskData);
                Thread.Sleep(100);
            }
        }

        private bool CanCreateBarrier()
        {
            return (Client != null) && (QueueID != Guid.Empty);
        }

        private void CreateBarrier()
        {
            Guid barrierID = Client.CreateBarrier(QueueID);
        }

        private bool CanRemoveQueue()
        {
            return (Client != null) && (QueueID != Guid.Empty);
        }

        private void RemoveQueue()
        {
            Client.RemoveQueue(QueueID);

            CurrentQueueID = Guid.Empty;
        }
        #endregion

        #region Event handlers

        private void ClientEventOccured(object sender, TaskQueueEventOccuredEventArgs args)
        {
            OccuredEventViewModel viewModel = new OccuredEventViewModel(args);

            EventsCollection.Add(viewModel);
        }
        #endregion
    }
}