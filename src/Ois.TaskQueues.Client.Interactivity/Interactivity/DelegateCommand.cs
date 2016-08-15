using System;

namespace Ois.TaskQueues.Client.Interactivity
{
    public class DelegateCommand : CommandBase
    {
        #region Fields

        private readonly Func<bool> Resolver;

        private readonly Action Executor;
        #endregion

        #region Constructors

        public DelegateCommand(Action executor, Func<bool> resolver = null)
        {
            Executor = executor; Resolver = resolver ?? AlwaysTrue;
        }
        #endregion

        #region CommandBase methods overriding

        public override bool CanExecute(object parameter)
        {
            return Resolver();
        }

        public override void Execute(object parameter)
        {
            Executor();
        }
        #endregion

        #region Protected class methods

        private static bool AlwaysTrue() => true;
        #endregion
    }
}