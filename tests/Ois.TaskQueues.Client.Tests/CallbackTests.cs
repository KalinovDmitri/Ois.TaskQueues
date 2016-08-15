using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ois.TaskQueues.Client.Tests
{
    using Communication;

    [TestClass]
    public class CallbackTests
    {
        #region Test methods

        [TestMethod]
        public void IsCallbackMethodExecutesSuccessfully()
        {
            TaskQueueClient client = new TaskQueueClient("net.tcp://DKalinov-PC:8788/tqservice");

            ManualResetEvent waitEvent = new ManualResetEvent(false);

            client.EventOccured += (s, args) =>
            {
                Console.WriteLine(args.EventType);
                Console.WriteLine(args.EventData);
                waitEvent.Set();
            };

            client.Register(false, TaskQueueServiceEvents.All);
            waitEvent.WaitOne();
        }
        #endregion
    }
}