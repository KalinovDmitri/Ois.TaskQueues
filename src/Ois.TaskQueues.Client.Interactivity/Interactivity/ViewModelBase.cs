using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ois.TaskQueues.Client.Interactivity
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors

        protected internal ViewModelBase() { }
        #endregion

        #region Class methods

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}