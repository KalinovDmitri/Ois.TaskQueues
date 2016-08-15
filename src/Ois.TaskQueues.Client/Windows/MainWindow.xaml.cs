using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ois.TaskQueues.Client.Windows
{
    using Interactivity;

    public partial class MainWindow : Window
    {
        #region Constants and fields

        private MainWindowViewModel ViewModel;
        #endregion

        #region Constructors

        public MainWindow() : base()
        {
            InitializeComponent();

            DataContext = ViewModel = new MainWindowViewModel();
        }
        #endregion

        #region Window methods overriding

        protected override void OnClosed(EventArgs args)
        {
            base.OnClosed(args);

            ViewModel.Dispose();
        }
        #endregion

        private void EventsGridRowLoading(object sender, DataGridRowEventArgs args)
        {
            int itemIndex = EventsGrid.Items.Count - 1;
            object item = EventsGrid.Items.GetItemAt(itemIndex);
            EventsGrid.ScrollIntoView(item);
        }
    }
}