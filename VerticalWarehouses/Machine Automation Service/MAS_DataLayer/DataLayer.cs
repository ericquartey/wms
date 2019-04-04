using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : BackgroundService, IDataLayer
    {
        #region Fields

        private readonly Task commadReceiveTask;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly DataLayerConfiguration dataLayerConfiguration;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private DataLayerContext primaryDataContext;

        private DataLayerContext secondaryDataContext;

        private CancellationToken stoppingToken;

        private bool suppressSecondary;

        #endregion

        #region Constructors

        public DataLayer(DataLayerConfiguration dataLayerConfiguration, DataLayerContext primaryDataContext, IEventAggregator eventAggregator, ILogger<DataLayer> logger)
        {
            if (primaryDataContext == null)
            {
                throw new ArgumentNullException();
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException();
            }

            if (logger == null)
            {
                throw new ArgumentNullException();
            }

            this.dataLayerConfiguration = dataLayerConfiguration;

            this.primaryDataContext = primaryDataContext;

            this.eventAggregator = eventAggregator;

            this.logger = logger;

            this.suppressSecondary = false;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commadReceiveTask = new Task(async () => await this.ReceiveCommandTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.ReceiveNotificationTaskFunction());

            //var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            //commandEvent.Subscribe(message => { this.commandQueue.Enqueue(message); },
            //    ThreadOption.PublisherThread,
            //    false,
            //    message => message.Destination == MessageActor.DataLayer || message.Destination == MessageActor.Any);

            //// The old WriteLogService
            //var NotificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            //NotificationEvent.Subscribe(message => { this.notificationQueue.Enqueue(message); },
            //    ThreadOption.PublisherThread,
            //    false,
            //    message => message.Destination == MessageActor.DataLayer || message.Destination == MessageActor.Any);

            //// INFO Log events
            //// INFO Command full events
            //var commandFullEvent = this.eventAggregator.GetEvent<CommandEvent>();
            //commandFullEvent.Subscribe(message => { this.LogMessages(message); },
            //    ThreadOption.PublisherThread,
            //    false);

            //// INFO Notification full events
            //var notificationFullEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            //notificationFullEvent.Subscribe(message => { this.LogMessages(message); },
            //    ThreadOption.PublisherThread,
            //    false);

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commadReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new DataLayerException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        /// <summary>
        /// This method is been invoked during the installation, to load the general_info.json file
        /// </summary>
        /// <param name="configurationFilePath">Configuration parameters to load</param>
        /// <exception cref="DataLayerExceptionCode.UNKNOWN_INFO_FILE_EXCEPTION">Exception for a wrong info file input name</exception>
        /// <exception cref="DataLayerExceptionCode.UNDEFINED_TYPE_EXCEPTION">Exception for an unknown data type</exception>
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

        private async void LogMessages(NotificationMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            var serializedData = JsonConvert.SerializeObject(message.Data);

            var logEntry = new LogEntry();

            logEntry.Data = serializedData;
            logEntry.Description = message.Description;
            logEntry.Destination = message.Destination.ToString();
            logEntry.ErrorLevel = message.ErrorLevel.ToString();
            logEntry.Source = message.Source.ToString();
            logEntry.Status = message.Status.ToString();
            logEntry.TimeStamp = DateTime.Now;
            logEntry.Type = message.Type.ToString();

            this.primaryDataContext.LogEntries.Add(logEntry);

            await this.primaryDataContext.SaveChangesAsync();
        }

        private async Task LogMessagesAsync(CommandMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            var serializedData = JsonConvert.SerializeObject(message.Data);

            var logEntry = new LogEntry();

            logEntry.Data = serializedData;
            logEntry.Description = message.Description;
            logEntry.Destination = message.Destination.ToString();
            logEntry.Source = message.Source.ToString();
            logEntry.TimeStamp = DateTime.Now;
            logEntry.Type = message.Type.ToString();

            this.primaryDataContext.LogEntries.Add(logEntry);

            await this.primaryDataContext.SaveChangesAsync();
        }

        private async Task primaryDataLayerInitializeAsync()
        {
            this.primaryDataContext.Database.Migrate();

            this.secondaryDataContext = new DataLayerContext(new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(this.dataLayerConfiguration.SecondaryConnectionString).Options);
            this.secondaryDataContext.Database.Migrate();

            try
            {
                await this.LoadConfigurationValuesInfoAsync(this.dataLayerConfiguration.ConfigurationFilePath);
            }
            catch (DataLayerException ex)
            {
                this.logger.LogError("Failed to load configuration values");
            }

            var errorNotification = new NotificationMessage(null,
                                                            "DataLayer initialization complete",
                                                            MessageActor.Any,
                                                            MessageActor.DataLayer,
                                                            MessageType.DataLayerReady,
                                                            MessageStatus.NoStatus);
            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
        }

        private async Task ReceiveCommandTaskFunction()
        {
            await this.primaryDataLayerInitializeAsync();

            await this.secondaryDataLayerInitializeAsync();

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
                    //TODO define action for each received notification
                    default:

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
                    //TODO define action for each received notification
                    default:

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
                throw new DataLayerException($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}", DataLayerExceptionCode.SaveData, ex);
            }
        }

        private async Task secondaryDataLayerInitializeAsync()
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
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Exception: {ex.Message} during the secondary DB initialization");
                throw new DataLayerException($"Exception: {ex.Message} during the secondary DB initialization", DataLayerExceptionCode.SaveData, ex);
            }
        }

        #endregion
    }
}
