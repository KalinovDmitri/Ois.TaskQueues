using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Ois.TaskQueues.Client.Interactivity
{
    /// <summary>
    /// Предоставляет абстрактную реализацию интерфейса <see cref="ICommand"/>
    /// </summary>
    public abstract class CommandBase : ICommand, INotifyPropertyChanged
    {
        #region Events

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CommandBase"/>
        /// </summary>
        protected internal CommandBase() { }
        #endregion

        #region Class methods

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}