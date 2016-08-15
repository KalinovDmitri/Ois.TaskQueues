using System;
using System.IO;
using System.Text;

using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;

namespace Ois.TaskQueues.Service.Modules
{
    public sealed class ConfigurationModule : Module
    {
        #region Constants and fields

        private const string ConfigurationDirectoryName = "Cfg";

        private const string ConfigurationFileName = "TaskQueueService.json";

        private readonly Type ConfigurationType;

        private readonly JsonSerializer Serializer;
        #endregion

        #region Constructors

        public ConfigurationModule()
        {
            ConfigurationType = typeof(TaskQueueServiceConfiguration);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new DefaultContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            settings.Converters.Add(new StringEnumConverter());

            Serializer = JsonSerializer.Create(settings);
        }
        #endregion

        #region Module methods overriding

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(CreateConfiguration).SingleInstance();
        }
        #endregion

        #region Private class methods

        private TaskQueueServiceConfiguration CreateConfiguration(IComponentContext context)
        {
            ILogger logger = context.Resolve<ILogger>();
            logger.Info("Service configuration initializing started...");

            TaskQueueServiceConfiguration configuration = null;

            string filePath = null, failMessage = null;
            bool isFileExists = TryGetConfigFile(out filePath, out failMessage);
            if (isFileExists)
            {
                using (TextReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    object result = Serializer.Deserialize(reader, ConfigurationType);

                    configuration = result as TaskQueueServiceConfiguration;
                }

                if (configuration != null)
                {
                    TaskQueueServiceConfiguration.SetInstance(configuration);
                    logger.Info("Configuration initialized successfully.");
                }
                else
                {
                    configuration = TaskQueueServiceConfiguration.Instance;
                    logger.Warn("Configuration reading error. Using default values.");
                }
            }
            else
            {
                configuration = TaskQueueServiceConfiguration.Instance;
                logger.Warn(failMessage + " Using default values.");
            }

            return configuration;
        }

        private bool TryGetConfigFile(out string configFilePath, out string failMessage)
        {
            configFilePath = string.Empty; failMessage = string.Empty;

            string location = GetType().Assembly.Location;
            DirectoryInfo parentDir = new FileInfo(location).Directory;

            if (parentDir == null)
            {
                failMessage = "Parent directory not exists.";
                return false;
            }

            DirectoryInfo configDir = new DirectoryInfo(Path.Combine(parentDir.FullName, ConfigurationDirectoryName));
            if (!configDir.Exists)
            {
                failMessage = $"Configuration directory '{configDir.FullName}' not exists.";
                return false;
            }

            FileInfo configFile = new FileInfo(Path.Combine(configDir.FullName, ConfigurationFileName));
            if (!configFile.Exists)
            {
                failMessage = $"Configuration file '{configFile.FullName}' not exists.";
                return false;
            }

            configFilePath = configFile.FullName;
            return true;
        }
        #endregion
    }
}