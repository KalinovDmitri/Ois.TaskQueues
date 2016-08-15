using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private bool UseTaskTemplateValue = true;

        private Guid ComputationID = Guid.Empty;
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

        public Guid CurrentComputationID
        {
            get { return ComputationID; }
            private set
            {
                ComputationID = value;
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

        #region Commands

        private DelegateCommand CommandRegisterClient;
        private DelegateCommand CommandCreateComputation;
        private DelegateCommand CommandCreateTask;
        private DelegateCommand CommandCreateBarrier;

        public ICommand RegisterClientCommand
        {
            get
            {
                return CommandRegisterClient ?? (CommandRegisterClient = new DelegateCommand(RegisterClient, CanRegisterClient));
            }
        }

        public ICommand CreateComputationCommand
        {
            get
            {
                return CommandCreateComputation ?? (CommandCreateComputation = new DelegateCommand(CreateComputation, CanCreateComputation));
            }
        }

        public ICommand CreateTaskCommand
        {
            get
            {
                return CommandCreateTask ?? (CommandCreateTask = new DelegateCommand(CreateTask, CanCreateTask));
            }
        }

        public ICommand CreateBarrierCommand
        {
            get
            {
                return CommandCreateBarrier ?? (CommandCreateBarrier = new DelegateCommand(CreateBarrier, CanCreateBarrier));
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
                Guid computationID = ComputationID;
                if (computationID != Guid.Empty)
                {
                    Client.FinishComputation(computationID);
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

        private bool CanRegisterClient()
        {
            return (Client != null) && !ClientConnectedValue;
        }

        private void RegisterClient()
        {
            ClientConnected = Client.Register(OnlyOwnEvents, SubscribedEvents);
        }

        private bool CanCreateComputation()
        {
            return (Client != null) && ClientConnectedValue;
        }

        private void CreateComputation()
        {
            CurrentComputationID = Client.CreateComputation();
        }

        private bool CanCreateTask()
        {
            return (Client != null) && (ComputationID != Guid.Empty);
        }

        private void CreateTask()
        {
            Guid taskID = Guid.Empty;

            if (UseTaskTemplateValue)
            {
                taskID = Client.CreateTask(ComputationID, TestTaskCategory, TestTaskData);
                return;
            }

            string[] taskInfo = TaskEditorWindow.Show();

            string taskCategory = taskInfo[0];
            string taskData = taskInfo[1];

            taskID = Client.CreateTask(ComputationID, taskCategory, taskData);
        }

        private bool CanCreateBarrier()
        {
            return (Client != null) && (ComputationID != Guid.Empty);
        }

        private void CreateBarrier()
        {
            Guid barrierID = Client.CreateBarrier(ComputationID);
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