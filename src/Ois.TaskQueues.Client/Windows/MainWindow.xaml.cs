using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;

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

            MainWindowViewModel viewModel = new MainWindowViewModel();
            viewModel.EventsCollectionChanged += new NotifyCollectionChangedEventHandler(EventsChanged);
            DataContext = ViewModel = viewModel;
        }
        #endregion

        #region Window methods overriding

        protected override void OnClosed(EventArgs args)
        {
            base.OnClosed(args);

            ViewModel.Dispose();
        }
        #endregion

        #region Event handlers

        private void EventsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                int itemIndex = (sender as IList).Count - 1;

                object item = EventsGrid.Items.GetItemAt(itemIndex);

                EventsGrid.ScrollIntoView(item);
            }
        }
        #endregion
    }
}