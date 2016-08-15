using System;

namespace Ois.TaskQueues.Client.Interactivity
{
    public class DelegateCommand<TArgument> : CommandBase
    {
        #region Fields

        private readonly Func<TArgument, bool> Resolver;

        private readonly Action<TArgument> Executor;
        #endregion

        #region Constructors

        public DelegateCommand(Action<TArgument> executor, Func<TArgument, bool> resolver = null)
        {
            Executor = executor; Resolver = resolver ?? AlwaysTrue;
        }
        #endregion

        #region CommandBase methods overriding

        public override bool CanExecute(object parameter)
        {
            if (parameter is TArgument)
            {
                return Resolver((TArgument)parameter);
            }

            TArgument argument = (TArgument)Convert.ChangeType(parameter, typeof(TArgument));
            if (argument != null)
            {
                return Resolver(argument);
            }

            return false;
        }

        public override void Execute(object parameter)
        {
            if (parameter is TArgument)
            {
                Executor((TArgument)parameter);
            }

            TArgument argument = (TArgument)Convert.ChangeType(parameter, typeof(TArgument));
            if (argument != null)
            {
                Executor(argument);
            }
        }
        #endregion

        #region Protected class methods

        private static bool AlwaysTrue(TArgument argument) => true;
        #endregion
    }
}