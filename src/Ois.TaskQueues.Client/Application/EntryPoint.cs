using System;

namespace Ois.TaskQueues.Client
{
    internal class EntryPoint
    {
        #region The entry point

        [STAThread]
        internal static void Main(string[] args)
        {
            new MainApplication().Run();
        }
        #endregion
    }
}