using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Autofac;

namespace Ois.TaskQueues.Service.Infrastructure.Tests
{
    [TestClass]
    public class TaskQueueServiceInfrastructureTests
    {
        #region Constants and fields

        private IContainer Container;
        #endregion

        #region Constructors

        public TaskQueueServiceInfrastructureTests() { }
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
        public void IsClientRegisteredSuccessfully()
        {
            var implementor = Container.Resolve<TaskQueueServiceImplementor>();

            var notificationService = Container.Resolve<TaskQueueNotificationService>();
            notificationService.Start();

            Guid clientID = Guid.NewGuid();
            var client = Container.Resolve<ITaskQueueClient>();
            
            bool result = implementor.RegisterClient(clientID, client);
            Assert.IsTrue(result);

            Thread.Sleep(200);

            notificationService.Dispose();
        }
        #endregion
    }
}