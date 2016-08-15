using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ois.TaskQueues.Client.Interactivity
{
    public class TaskEditorWindow : Window
    {
        #region Constants

        private const string TestTaskCategory = "TestCategory";

        private const string WindowTitle = "Ввод данных задачи";

        private const string MarkupUriLocator = "/TaskQueueClient;component/interactivity/taskeditorwindow.xaml";
        #endregion

        #region Static methods

        public static string[] Show(double height = 700.0, double width = 500.0)
        {
            TaskEditorWindow editor = new TaskEditorWindow
            {
                Height = height,
                Width = width,
                Title = WindowTitle
            };

            bool result = editor.ShowDialog().GetValueOrDefault();
            if (result)
            {
                return new []
                {
                    editor.TaskCategoryValue,
                    editor.TaskDataValue
                };
            }

            return new [] { string.Empty, string.Empty };
        }
        #endregion

        #region Commands

        private DelegateCommand CommandAccept;
        private DelegateCommand CommandDecline;

        public ICommand AcceptCommand
        {
            get
            {
                return CommandAccept ?? (CommandAccept = new DelegateCommand(AcceptChanges, CanAcceptChanges));
            }
        }

        public ICommand DeclineCommand
        {
            get
            {
                return CommandDecline ?? (CommandDecline = new DelegateCommand(DeclineChanges));
            }
        }
        #endregion

        #region Properties

        private string TaskCategoryValue = TestTaskCategory;
        private string TaskDataValue;

        public string TaskCategory
        {
            get { return TaskCategoryValue; }
            set { TaskCategoryValue = value; }
        }

        public string TaskData
        {
            get { return TaskDataValue; }
            set { TaskDataValue = value; }
        }
        #endregion

        #region Constructors

        private TaskEditorWindow() : base()
        {
            Uri resourceLocator = new Uri(MarkupUriLocator, UriKind.Relative);
            Application.LoadComponent(this, resourceLocator);

            DataContext = this;
        }
        #endregion

        #region Private class methods

        private bool CanAcceptChanges()
        {
            return !string.IsNullOrEmpty(TaskCategoryValue) && !string.IsNullOrEmpty(TaskDataValue);
        }

        private void AcceptChanges()
        {
            DialogResult = true;
        }

        private void DeclineChanges()
        {
            DialogResult = false;
        }
        #endregion
    }
}