using System;
using System.Net;
using System.Net.Sockets;
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
            TcpClient client = new TcpClient();
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 8788);

            try
            {
                client.Connect(endpoint);
                if (client.Connected)
                {
                    TaskQueueClient taskClient = new TaskQueueClient("net.tcp://DKalinov-PC:8788/tqservice");

                    ManualResetEvent waitEvent = new ManualResetEvent(false);

                    taskClient.EventOccured += (s, args) =>
                    {
                        Console.WriteLine(args.EventType);
                        Console.WriteLine(args.EventData);
                        waitEvent.Set();
                    };

                    taskClient.Register(false, TaskQueueServiceEvents.All);
                    waitEvent.WaitOne();
                }
            }
            catch (SocketException socketExc)
            {
                Console.WriteLine(socketExc);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            finally
            {
                if (client.Connected)
                {
                    client.Close();
                }
                client = null;
            }
        }
        #endregion
    }
}