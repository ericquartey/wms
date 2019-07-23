using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Models;
using Ferretto.VW.MAS.Utils.Utilities;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Prism.Events;
using RuntimeValue = Ferretto.VW.MAS.DataLayer.Models.RuntimeValue;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : BackgroundService, IDataLayer
    {
        #region Fields

        private readonly Task applicationLogWriteTask;

        private readonly BlockingConcurrentQueue<CommandMessage> commandLogQueue;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly DataLayerConfiguration dataLayerConfiguration;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationLogQueue;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly SetupStatusVolatile setupStatusVolatile;

        private DbContextOptions<DataLayerContext> primaryContextOptions;

        private DbContextOptions<DataLayerContext> secondaryContextOptions;

        private CancellationToken stoppingToken;

        private bool suppressSecondary;

        #endregion

        #region Constructors

        public DataLayer(DataLayerConfiguration dataLayerConfiguration, IEventAggregator eventAggregator, ILogger<DataLayer> logger)
        {
            if (dataLayerConfiguration == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(nameof(dataLayerConfiguration)), string.Empty, 0));
                return;
            }

            if (eventAggregator == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(nameof(eventAggregator)), string.Empty, 0));
                return;
            }

            if (logger == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(nameof(logger)), string.Empty, 0));
                return;
            }

            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.primaryContextOptions = new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(this.dataLayerConfiguration.PrimaryConnectionString).Options;

            this.secondaryContextOptions = new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(this.dataLayerConfiguration.SecondaryConnectionString).Options;

            this.dataLayerConfiguration = dataLayerConfiguration;

            this.suppressSecondary = false;

            this.setupStatusVolatile = new SetupStatusVolatile();

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandLogQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationLogQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(this.ReceiveCommandTaskFunction);
            this.notificationReceiveTask = new Task(this.ReceiveNotificationTaskFunction);
            this.applicationLogWriteTask = new Task(this.ApplicationLogWriterTaskFunction);

            var commandLogEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandLogEvent.Subscribe(
                commandMessage => { this.commandLogQueue.Enqueue(commandMessage); },
                ThreadOption.PublisherThread,
                false);

            var notificationLogEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationLogEvent.Subscribe(
                notificationMessage => { this.notificationLogQueue.Enqueue(notificationMessage); },
                ThreadOption.PublisherThread,
                false);

            this.logger?.LogInformation("DataLayer Constructor");
        }

        #endregion

        #region Methods

        public void switchDBContext()
        {
            lock (this.primaryContextOptions)
            {
                var switchContextOptions = this.primaryContextOptions;
                lock (this.secondaryContextOptions)
                {
                    this.primaryContextOptions = this.secondaryContextOptions;
                    this.secondaryContextOptions = switchContextOptions;
                }
            }

            this.suppressSecondary = true;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
                this.applicationLogWriteTask.Start();
            }
            catch (Exception ex)
            {
                this.SendMessage(new DLExceptionMessageData(ex, ex.Message, 0));
            }

            return Task.CompletedTask;
        }

        private void DataLayerInitialize()
        {
            lock (this.primaryContextOptions)
            {
                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryDataContext.Database.Migrate();
                }
            }

            lock (this.secondaryContextOptions)
            {
                using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                {
                    secondaryDataContext.Database.Migrate();
                }
            }

            this.suppressSecondary = true;

            try
            {
                this.LoadConfigurationValuesInfo(this.dataLayerConfiguration.ConfigurationFilePath);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Exception: {ex.Message} while loading configuration values");
                this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
                return;
            }

            this.SecondaryDataLayerInitialize();

            this.suppressSecondary = false;

            var errorNotification = new NotificationMessage(
                null,
                                                            "DataLayer initialization complete",
                                                            MessageActor.Any,
                                                            MessageActor.DataLayer,
                                                            MessageType.DataLayerReady,
                                                            MessageStatus.NoStatus);
            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
        }

        /// <summary>
        /// This method is been invoked during the installation, to load the general_info.json file
        /// </summary>
        /// <param name="configurationFilePath">Configuration parameters to load</param>
        private void LoadConfigurationValuesInfo(string configurationFilePath)
        {
            using (var streamReader = new StreamReader(configurationFilePath))
            {
                var json = streamReader.ReadToEnd();
                var jsonObject = JObject.Parse(json);

                foreach (var jsonCategory in jsonObject)
                {
                    if (!Enum.TryParse(jsonCategory.Key, false, out ConfigurationCategory jsonElementCategory))
                    {
                        throw new DataLayerException($"Invalid configuration category: {jsonCategory.Key} found in configuration file");
                    }

                    foreach (var jsonData in (JObject)jsonCategory.Value)
                    {
                        switch (jsonElementCategory)
                        {
                            case ConfigurationCategory.GeneralInfo:
                                if (!Enum.TryParse(jsonData.Key, false, out GeneralInfo generalInfoData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)generalInfoData, jsonData.Value);

                                break;

                            case ConfigurationCategory.SetupNetwork:
                                if (!Enum.TryParse(jsonData.Key, false, out SetupNetwork setupNetworkData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)setupNetworkData, jsonData.Value);

                                break;

                            case ConfigurationCategory.SetupStatus:
                                if (!Enum.TryParse(jsonData.Key, false, out SetupStatus setupStatusData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)setupStatusData, jsonData.Value);

                                break;

                            case ConfigurationCategory.VerticalAxis:
                                if (!Enum.TryParse(jsonData.Key, false, out VerticalAxis verticalAxisData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)verticalAxisData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalAxis:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalAxis horizontalAxisData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)horizontalAxisData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalMovementForwardProfile:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalMovementForwardProfile horizontalMovementForwardProfileData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)horizontalMovementForwardProfileData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalMovementBackwardProfile:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalMovementBackwardProfile horizontalMovementBackwardProfileData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)horizontalMovementBackwardProfileData, jsonData.Value);

                                break;

                            case ConfigurationCategory.VerticalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out VerticalManualMovements verticalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)verticalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalManualMovements horizontalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)horizontalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BeltBurnishing:
                                if (!Enum.TryParse(jsonData.Key, false, out BeltBurnishing beltBurnishingData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)beltBurnishingData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ResolutionCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out ResolutionCalibration resolutionCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)resolutionCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.OffsetCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out OffsetCalibration offsetCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)offsetCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.CellControl:
                                if (!Enum.TryParse(jsonData.Key, false, out CellControl cellControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)cellControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.PanelControl:
                                if (!Enum.TryParse(jsonData.Key, false, out PanelControl panelControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)panelControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ShutterHeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out ShutterHeightControl shutterHeightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)shutterHeightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.WeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out WeightControl weightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)weightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BayPositionControl:
                                if (!Enum.TryParse(jsonData.Key, false, out BayPositionControl bayPositionControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)bayPositionControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.LoadFirstDrawer:
                                if (!Enum.TryParse(jsonData.Key, false, out LoadFirstDrawer loadFirstDrawerData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)loadFirstDrawerData, jsonData.Value);

                                break;
                        }
                    }
                }
            }
        }

        private void ReceiveCommandTaskFunction()
        {
            this.DataLayerInitialize();

            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.NoType:
                        break;

                    case MessageType.Homing:
                        break;

                    case MessageType.Stop:
                        break;

                    case MessageType.SensorsChanged:
                        break;

                    case MessageType.DataLayerReady:
                        break;

                    case MessageType.SwitchAxis:
                        break;

                    case MessageType.CalibrateAxis:
                        break;

                    case MessageType.ShutterControl:
                        break;

                    case MessageType.MissionAdded:
                        break;

                    case MessageType.CreateMission:
                        break;

                    case MessageType.Positioning:
                        break;
                }

                lock (this.primaryContextOptions)
                {
                    using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                    {
                        primaryDataContext.SaveChanges();
                    }
                }

                lock (this.secondaryContextOptions)
                {
                    using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                    {
                        secondaryDataContext.SaveChanges();
                    }
                }
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private void ReceiveNotificationTaskFunction()
        {
            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.NoType:
                        break;

                    case MessageType.Homing:
                        break;

                    case MessageType.Stop:
                        break;

                    case MessageType.SensorsChanged:
                        break;

                    case MessageType.DataLayerReady:
                        break;

                    case MessageType.SwitchAxis:
                        break;

                    case MessageType.CalibrateAxis:
                        break;

                    case MessageType.ShutterControl:
                        break;

                    case MessageType.MissionAdded:
                        break;

                    case MessageType.CreateMission:
                        break;

                    case MessageType.Positioning:
                        break;
                }

                lock (this.primaryContextOptions)
                {
                    using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                    {
                        primaryDataContext.SaveChanges();
                    }
                }

                lock (this.secondaryContextOptions)
                {
                    using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                    {
                        secondaryDataContext.SaveChanges();
                    }
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void SaveConfigurationData(ConfigurationCategory elementCategory, long configurationData, JToken jsonDataValue)
        {
            if (!Enum.TryParse(jsonDataValue.Type.ToString(), false, out ConfigurationDataType generalInfoConfigurationDataType))
            {
                throw new DataLayerException($"Invalid configuration data type: {jsonDataValue.Type.ToString()} for data {configurationData} in section {elementCategory} found in configuration file");
            }

            try
            {
                switch (generalInfoConfigurationDataType)
                {
                    case ConfigurationDataType.Boolean:
                        this.SetBoolConfigurationValue(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<bool>());
                        break;

                    case ConfigurationDataType.Date:
                        this.SetDateTimeConfigurationValue(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<DateTime>());
                        break;

                    case ConfigurationDataType.Integer:
                        this.SetIntegerConfigurationValue(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<int>());
                        break;

                    case ConfigurationDataType.Float:
                        this.SetDecimalConfigurationValue(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<decimal>());
                        break;

                    case ConfigurationDataType.String:
                        var stringValue = jsonDataValue.Value<string>();
                        if (IPAddress.TryParse(stringValue, out var configurationValue))
                        {
                            this.SetIpAddressConfigurationValue(configurationData, (long)elementCategory, configurationValue);
                        }
                        else
                        {
                            this.SetStringConfigurationValue(configurationData, (long)elementCategory, stringValue);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}");

                //TEMP throw new DataLayerException($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}", DataLayerExceptionCode.SaveData, ex);
                this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
            }
        }

        private void SecondaryDataLayerInitialize()
        {
            try
            {
                DbSet<ConfigurationValue> configurationValues;
                DbSet<Cell> cells;
                DbSet<FreeBlock> freeBlocks;
                DbSet<LoadingUnit> loadingUnits;
                DbSet<LogEntry> logEntries;
                DbSet<RuntimeValue> runtimeValues;

                lock (this.primaryContextOptions)
                {
                    using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                    {
                        configurationValues = primaryDataContext.ConfigurationValues;

                        cells = primaryDataContext.Cells;

                        freeBlocks = primaryDataContext.FreeBlocks;

                        loadingUnits = primaryDataContext.LoadingUnits;

                        logEntries = primaryDataContext.LogEntries;

                        runtimeValues = primaryDataContext.RuntimeValues;
                    }
                }

                lock (this.secondaryContextOptions)
                {
                    using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                    {
                        secondaryDataContext.ConfigurationValues.AddRange(configurationValues);

                        secondaryDataContext.Cells.AddRange(cells);

                        secondaryDataContext.FreeBlocks.AddRange(freeBlocks)
                            ;
                        secondaryDataContext.LoadingUnits.AddRange(loadingUnits);

                        secondaryDataContext.LogEntries.AddRange(logEntries);

                        secondaryDataContext.RuntimeValues.AddRange(runtimeValues);

                        secondaryDataContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Exception: {ex.Message} during the secondary DB initialization");

                this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
            }
        }

        private void SendMessage(IMessageData data)
        {
            var msg = new NotificationMessage(
                data,
                "DataLayer Error",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DLException,
                MessageStatus.OperationError,
                ErrorLevel.Critical);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        #endregion
    }
}
