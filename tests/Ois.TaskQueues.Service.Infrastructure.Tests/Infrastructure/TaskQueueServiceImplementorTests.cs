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
        }
        #endregion
    }
}