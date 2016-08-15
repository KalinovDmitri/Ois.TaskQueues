using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Autofac;

namespace Ois.TaskQueues.Service.Modules.Tests
{
    [TestClass]
    public class ConfigurationModuleTests
    {
        #region Constants and fields

        private IContainer Container;
        #endregion

        #region Constructors

        public ConfigurationModuleTests() { }
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
        public void IsConfigurationCreatedSuccessfully()
        {
            TaskQueueServiceConfiguration configuration = Container.Resolve<TaskQueueServiceConfiguration>();

            Assert.IsNotNull(configuration);
            Assert.AreEqual(8788, configuration.ClientServicePort);
            Assert.AreEqual(8789, configuration.WorkerServicePort);
            Assert.AreEqual(30.0, configuration.ExecutionTimeout.TotalSeconds);
        }
        #endregion
    }
}