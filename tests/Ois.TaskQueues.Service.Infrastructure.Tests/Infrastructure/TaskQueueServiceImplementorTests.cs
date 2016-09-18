using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Autofac;

namespace Ois.TaskQueues.Service.Infrastructure.Tests
{
    [TestClass]
    public class TaskQueueServiceImplementorTests
    {
        #region Constants and fields

        private IContainer Container;
        #endregion

        #region Constructors

        public TaskQueueServiceImplementorTests() { }
        #endregion

        #region Initialize / cleanup methods

        [TestInitialize]
        public void Initialize()
        {
            Container = TaskQueueServiceContainerFactory.CreateContainer();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Container.Dispose();
        }
        #endregion

        #region Test methods

        [TestMethod]
        public void IsServiceImplementorInitializedSuccessfully()
        {
            var implementor = Container.Resolve<TaskQueueServiceImplementor>();
            
            Assert.IsNotNull(implementor);
            Assert.IsNotNull(implementor.BalancingService);
            Assert.IsNotNull(implementor.ClientService);
            Assert.IsNotNull(implementor.QueueService);
            Assert.IsNotNull(implementor.NotificationService);
            Assert.IsNotNull(implementor.ProcessingService);
            Assert.IsNotNull(implementor.WorkerService);
        }

        [TestMethod]
        public void IsQueueLifecycleWorksSuccessfully()
        {
            var implementor = Container.Resolve<TaskQueueServiceImplementor>();
            var client = Container.Resolve<TaskQueueClientMock>();

            Guid clientID = client.ClientID;

            bool registered = implementor.RegisterClient(clientID, client);
            Assert.IsTrue(registered);

            Guid queueID = implementor.CreateQueue(clientID);
            Assert.AreNotEqual(Guid.Empty, queueID);

            Guid taskID = implementor.AddTask(queueID, "TEST", "{}");
            Assert.AreNotEqual(Guid.Empty, taskID);

            implementor.AcknowledgeTask(Guid.Empty, taskID);

            implementor.RemoveQueue(queueID);

            bool unregistered = implementor.UnregisterClient(clientID);
            Assert.IsTrue(unregistered);
        }
        #endregion
    }
}