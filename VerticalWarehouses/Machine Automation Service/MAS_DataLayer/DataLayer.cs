using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember

namespace Ferretto.VW.MAS_DataLayer
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

        private DataLayerContext primaryDataContext;

        private DataLayerContext secondaryDataContext;

        private SetupStatusVolatile setupStatusVolatile;

        private CancellationToken stoppingToken;

        private bool suppressSecondary;

        #endregion

        #region Constructors

        public DataLayer(DataLayerConfiguration dataLayerConfiguration, DataLayerContext primaryDataContext, IEventAggregator eventAggregator, ILogger<DataLayer> logger)
        {
            if (primaryDataContext == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(), "", 0));
            }

            if (eventAggregator == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(), "", 0));
            }

            if (logger == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(), "", 0));
            }

            this.dataLayerConfiguration = dataLayerConfiguration;

            this.primaryDataContext = primaryDataContext;

            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.suppressSecondary = false;

            this.setupStatusVolatile = new SetupStatusVolatile();

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandLogQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationLogQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(async () => await this.ReceiveCommandTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.ReceiveNotificationTaskFunction());
            this.applicationLogWriteTask = new Task(async () => await this.ApplicationLogWriterTaskFunction());

            var commandLogEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandLogEvent.Subscribe(commandMessage => { this.commandLogQueue.Enqueue(commandMessage); },
                ThreadOption.PublisherThread,
                false);

            var notificationLogEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationLogEvent.Subscribe(notificationMessage => { this.notificationLogQueue.Enqueue(notificationMessage); },
                ThreadOption.PublisherThread,
                false);

            this.logger?.LogInformation("DataLayer Constructor");
        }

        #endregion

        #region Methods

        public void switchDBContext()
        {
            DataLayerContext switchDataContext;

            switchDataContext = this.primaryDataContext;
            this.primaryDataContext = this.secondaryDataContext;
            this.secondaryDataContext = switchDataContext;

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
                //TEMP throw new DataLayerException($"Exception: {ex.Message} while starting service threads", ex);
                this.SendMessage(new DLExceptionMessageData(ex, "", 0));
            }

            return Task.CompletedTask;
        }

        private async Task DataLayerInitializeAsync()
        {
            this.primaryDataContext.Database.Migrate();

            this.secondaryDataContext = new DataLayerContext(new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(this.dataLayerConfiguration.SecondaryConnectionString).Options);
            this.secondaryDataContext.Database.Migrate();

            this.suppressSecondary = true;

            try
            {
                await this.LoadConfigurationValuesInfoAsync(this.dataLayerConfiguration.ConfigurationFilePath);
            }
            //TEMP catch (DataLayerException ex)
            catch (Exception ex)
            {
                this.logger.LogError($"Exception: {ex.Message} while loading configuration values");
                this.SendMessage(new DLExceptionMessageData(ex, "", 0));
            }

            await this.SecondaryDataLayerInitializeAsync();

            this.suppressSecondary = false;

            var errorNotification = new NotificationMessage(null,
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
        /// <exception cref="DataLayerExceptionCode.UnknownInfoFileException">Exception for a wrong info file input name</exception>
        /// <exception cref="DataLayerExceptionCode.UndefinedTypeException">Exception for an unknown data type</exception>
        private async Task LoadConfigurationValuesInfoAsync(string configurationFilePath)
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

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)generalInfoData, jsonData.Value);

                                break;

                            case ConfigurationCategory.SetupNetwork:
                                if (!Enum.TryParse(jsonData.Key, false, out SetupNetwork setupNetworkData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)setupNetworkData, jsonData.Value);

                                break;

                            case ConfigurationCategory.SetupStatus:
                                if (!Enum.TryParse(jsonData.Key, false, out SetupStatus setupStatusData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)setupStatusData, jsonData.Value);

                                break;

                            case ConfigurationCategory.VerticalAxis:
                                if (!Enum.TryParse(jsonData.Key, false, out VerticalAxis verticalAxisData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)verticalAxisData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalAxis:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalAxis horizontalAxisData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalAxisData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalMovementForwardProfile:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalMovementForwardProfile horizontalMovementForwardProfileData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalMovementForwardProfileData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalMovementBackwardProfile:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalMovementBackwardProfile horizontalMovementBackwardProfileData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalMovementBackwardProfileData, jsonData.Value);

                                break;

                            case ConfigurationCategory.VerticalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out VerticalManualMovements verticalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)verticalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalManualMovements horizontalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BeltBurnishing:
                                if (!Enum.TryParse(jsonData.Key, false, out BeltBurnishing beltBurnishingData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)beltBurnishingData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ResolutionCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out ResolutionCalibration resolutionCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)resolutionCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.OffsetCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out OffsetCalibration offsetCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)offsetCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.CellControl:
                                if (!Enum.TryParse(jsonData.Key, false, out CellControl cellControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)cellControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.PanelControl:
                                if (!Enum.TryParse(jsonData.Key, false, out PanelControl panelControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)panelControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ShutterHeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out ShutterHeightControl shutterHeightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)shutterHeightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.WeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out WeightControl weightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)weightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BayPositionControl:
                                if (!Enum.TryParse(jsonData.Key, false, out BayPositionControl bayPositionControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)bayPositionControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.LoadFirstDrawer:
                                if (!Enum.TryParse(jsonData.Key, false, out LoadFirstDrawer loadFirstDrawerData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)loadFirstDrawerData, jsonData.Value);

                                break;
                        }
                    }
                }
            }
        }

        private async Task ReceiveCommandTaskFunction()
        {
            await this.DataLayerInitializeAsync();

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

                    case MessageType.AddMission:
                        break;

                    case MessageType.CreateMission:
                        break;

                    case MessageType.Positioning:
                        break;

                    case MessageType.VerticalPositioning:
                        break;
                }

                await this.primaryDataContext.SaveChangesAsync(this.stoppingToken);
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task ReceiveNotificationTaskFunction()
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

                    case MessageType.AddMission:
                        break;

                    case MessageType.CreateMission:
                        break;

                    case MessageType.Positioning:
                        break;

                    case MessageType.VerticalPositioning:
                        break;
                }

                await this.primaryDataContext.SaveChangesAsync(this.stoppingToken);
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        private async Task SaveConfigurationDataAsync(ConfigurationCategory elementCategory, long configurationData, JToken jsonDataValue)
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
                        await this.SetBoolConfigurationValueAsync(configurationData, (long)elementCategory,
                            jsonDataValue.Value<bool>());
                        break;

                    case ConfigurationDataType.Date:
                        await this.SetDateTimeConfigurationValueAsync(configurationData, (long)elementCategory,
                            jsonDataValue.Value<DateTime>());
                        break;

                    case ConfigurationDataType.Integer:
                        await this.SetIntegerConfigurationValueAsync(configurationData, (long)elementCategory,
                            jsonDataValue.Value<int>());
                        break;

                    case ConfigurationDataType.Float:
                        await this.SetDecimalConfigurationValueAsync(configurationData, (long)elementCategory,
                            jsonDataValue.Value<decimal>());
                        break;

                    case ConfigurationDataType.String:
                        var stringValue = jsonDataValue.Value<string>();
                        if (IPAddress.TryParse(stringValue, out var configurationValue))
                        {
                            await this.SetIPAddressConfigurationValueAsync(configurationData, (long)elementCategory, configurationValue);
                        }
                        else
                        {
                            await this.SetStringConfigurationValueAsync(configurationData, (long)elementCategory, stringValue);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}");
                //TEMP throw new DataLayerException($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}", DataLayerExceptionCode.SaveData, ex);
                this.SendMessage(new DLExceptionMessageData(ex, "", 0));
            }
        }

        private async Task SecondaryDataLayerInitializeAsync()
        {
            var secondaryInitialized = await this.secondaryDataContext.ConfigurationValues.AnyAsync(cancellationToken: this.stoppingToken);

            if (!secondaryInitialized)
            {
                try
                {
                    var configurationValues = this.primaryDataContext.ConfigurationValues;
                    await this.secondaryDataContext.ConfigurationValues.AddRangeAsync(configurationValues);

                    var cells = this.primaryDataContext.Cells;
                    await this.secondaryDataContext.Cells.AddRangeAsync(cells);

                    var freeBlocks = this.primaryDataContext.FreeBlocks;
                    await this.secondaryDataContext.FreeBlocks.AddRangeAsync(freeBlocks);

                    var loadingUnits = this.primaryDataContext.LoadingUnits;
                    await this.secondaryDataContext.LoadingUnits.AddRangeAsync(loadingUnits);

                    var logEntries = this.primaryDataContext.LogEntries;
                    await this.secondaryDataContext.LogEntries.AddRangeAsync(logEntries);

                    var runtimeValues = this.primaryDataContext.RuntimeValues;
                    await this.secondaryDataContext.RuntimeValues.AddRangeAsync(runtimeValues);

                    await this.secondaryDataContext.SaveChangesAsync(this.stoppingToken);
                }
                catch (Exception ex)
                {
                    this.logger.LogCritical($"Exception: {ex.Message} during the secondary DB initialization");
                    //TEMP throw new DataLayerException($"Exception: {ex.Message} during the secondary DB initialization", DataLayerExceptionEnum.SaveData, ex);

                    this.SendMessage(new DLExceptionMessageData(ex, "", 0));
                }
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
