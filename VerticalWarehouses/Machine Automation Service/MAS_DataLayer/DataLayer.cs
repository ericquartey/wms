using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : BackgroundService, IDataLayer
    {
        #region Fields

        private readonly Task commadReceiveTask;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly IEventAggregator eventAggregator;

        private readonly IOptions<FilesInfo> filesInfo;

        private readonly DataLayerContext inMemoryDataContext;

        private readonly ILogger logger;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public DataLayer(string connectionString, DataLayerContext inMemoryDataContext, IEventAggregator eventAggregator, IOptions<FilesInfo> filesInfo, ILogger<DataLayer> logger)
        {
            if (inMemoryDataContext == null)
            {
                throw new ArgumentNullException();
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException();
            }

            if (filesInfo == null)
            {
                throw new ArgumentNullException();
            }

            if (logger == null)
            {
                throw new ArgumentNullException();
            }

            this.inMemoryDataContext = inMemoryDataContext;

            this.eventAggregator = eventAggregator;

            this.filesInfo = filesInfo;

            this.logger = logger;

            using (var initialContext = new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options))
            {
                initialContext.Database.Migrate();

                if (!initialContext.ConfigurationValues.Any())
                {
                    //this.LoadConfigurationValuesInfo(InfoFilesEnum.GeneralInfo);
                    //this.LoadConfigurationValuesInfo(InfoFilesEnum.InstallationInfo);
                }

                this.LoadConfigurationValuesInfo(InfoFilesEnum.GeneralInfo);
                this.LoadConfigurationValuesInfo(InfoFilesEnum.InstallationInfo);

                foreach (var configurationValue in initialContext.ConfigurationValues)
                {
                    this.inMemoryDataContext.ConfigurationValues.Add(configurationValue);
                }

                this.inMemoryDataContext.SaveChanges();
            }

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commadReceiveTask = new Task(async () => await this.ReceiveCommandTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.ReceiveNotificationTaskFunction());

            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(message => { this.commandQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.DataLayer || message.Destination == MessageActor.Any);

            // The old WriteLogService
            var NotificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            NotificationEvent.Subscribe(message => { this.notificationQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.DataLayer || message.Destination == MessageActor.Any);

            // INFO Log events
            // INFO Command full events
            //var commandFullEvent = this.eventAggregator.GetEvent<CommandEvent>();
            //commandFullEvent.Subscribe(message => { this.LogMessages(message); },
            //    ThreadOption.PublisherThread,
            //    false);

            // INFO Notification full events
            //var notificationFullEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            //notificationFullEvent.Subscribe(message => { this.LogMessages(message); },
            //    ThreadOption.PublisherThread,
            //    false);

            //this.logger?.LogInformation("DataLayer Constructor");

            var commandMessage = new CommandMessage();
            commandMessage.Source = MessageActor.DataLayer;
            commandMessage.Destination = MessageActor.DataLayer;

            this.LogMessages(commandMessage);
        }

        /// <summary>
        /// FAKE constructor to be used EXCLUSIVELLY for unit testing
        /// </summary>
        /// <param name="inMemoryDataContext"></param>
        /// <param name="eventAggregator"></param>
        public DataLayer(DataLayerContext inMemoryDataContext, IEventAggregator eventAggregator, IOptions<FilesInfo> filesInfo)
        {
            if (inMemoryDataContext == null)
            {
                throw new ArgumentNullException();
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException();
            }

            if (filesInfo == null)
            {
                throw new ArgumentNullException();
            }

            this.inMemoryDataContext = inMemoryDataContext;

            this.eventAggregator = eventAggregator;

            this.filesInfo = filesInfo;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();

            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commadReceiveTask = new Task(async () => await this.ReceiveCommandTaskFunction());
            this.notificationReceiveTask = new Task(async () => await this.ReceiveNotificationTaskFunction());

            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(message => { this.commandQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.DataLayer || message.Destination == MessageActor.Any);

            // The old WriteLogService
            var NotificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            NotificationEvent.Subscribe(message => { this.notificationQueue.Enqueue(message); },
                ThreadOption.PublisherThread,
                false,
                message => message.Destination == MessageActor.DataLayer || message.Destination == MessageActor.Any);

            // INFO Log events
            // INFO Command full events
            var commandFullEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandFullEvent.Subscribe(message => { this.LogMessages(message); },
                ThreadOption.PublisherThread,
                false);

            // INFO Notification full events
            var notificationFullEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationFullEvent.Subscribe(message => { this.LogMessages(message); },
                ThreadOption.PublisherThread,
                false);
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void LoadConfigurationValuesInfo(InfoFilesEnum configurationValueRequest)
        {
            string requestPath;

            switch (configurationValueRequest)
            {
                case InfoFilesEnum.GeneralInfo:
                    {
                        requestPath = this.filesInfo.Value.GeneralInfoPath;
                        break;
                    }
                case InfoFilesEnum.InstallationInfo:
                    {
                        requestPath = this.filesInfo.Value.InstallationInfoPath;
                        break;
                    }
                default:
                    {
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.UNKNOWN_INFO_FILE_EXCEPTION);
                    }
            }

            using (var streamReader = new StreamReader(requestPath))
            {
                var json = streamReader.ReadToEnd();
                var jsonObject = JObject.Parse(json);

                DataTypeEnum jsonElementType;
                ConfigurationCategoryValueEnum jsonElementCategory;

                foreach (var jsonElement in jsonObject)
                {
                    jsonElementCategory = this.GetJSonElementConfigurationCategory(jsonElement);
                    long elementEnumerationID;
                    switch (jsonElementCategory)
                    {
                        case ConfigurationCategoryValueEnum.GeneralInfoEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((GeneralInfoEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.SetupNetworkEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((SetupNetworkEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.SetupStatusEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((SetupStatusEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.VerticalAxisEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((VerticalAxisEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.HorizontalAxisEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((HorizontalAxisEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.HorizontalMovementForwardProfileEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((HorizontalMovementForwardProfileEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.HorizontalMovementBackwardProfileEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((HorizontalMovementBackwardProfileEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.VerticalManualMovementsEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((VerticalManualMovementsEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.HorizontalManualMovementsEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((HorizontalManualMovementsEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.BeltBurnishingEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((BeltBurnishingEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.ResolutionCalibrationEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((ResolutionCalibrationEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.OffsetCalibrationEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((OffsetCalibrationEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.CellControlEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((CellControlEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.PanelControlEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((PanelControlEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.ShutterHeightControlEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((ShutterHeightControlEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.WeightControlEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((WeightControlEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.BayPositionControlEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((BayPositionControlEnum)elementEnumerationID);
                            break;

                        case ConfigurationCategoryValueEnum.LoadFirstDrawerEnum:
                            elementEnumerationID = (long)jsonElement.Value;
                            jsonElementType = this.ConvertConfigurationValue((LoadFirstDrawerEnum)elementEnumerationID);
                            break;

                        default:
                            jsonElementType = DataTypeEnum.UndefinedType;
                            elementEnumerationID = 0;
                            break;
                    }

                    switch (jsonElementType)
                    {
                        case (DataTypeEnum.booleanType):
                            {
                                this.SetBoolConfigurationValue(elementEnumerationID, (long)jsonElementCategory, (bool)jsonElement.Value.ToObject(typeof(bool)));
                                break;
                            }
                        case (DataTypeEnum.dateTimeType):
                            {
                                this.SetDateTimeConfigurationValue(elementEnumerationID, (long)jsonElementCategory, (DateTime)jsonElement.Value.ToObject(typeof(DateTime)));
                                break;
                            }
                        case (DataTypeEnum.decimalType):
                            {
                                this.SetDecimalConfigurationValue(elementEnumerationID, (long)jsonElementCategory, (decimal)jsonElement.Value.ToObject(typeof(decimal)));
                                break;
                            }
                        case (DataTypeEnum.integerType):
                            {
                                this.SetIntegerConfigurationValue(elementEnumerationID, (long)jsonElementCategory, (int)jsonElement.Value.ToObject(typeof(int)));
                                break;
                            }
                        case (DataTypeEnum.stringType):
                            {
                                this.SetStringConfigurationValue(elementEnumerationID, (long)jsonElementCategory, jsonElement.Value.ToString());
                                break;
                            }
                        default:
                            {
                                throw new InMemoryDataLayerException(DataLayerExceptionEnum.UNDEFINED_TYPE_EXCEPTION);
                            }
                    }
                }
            }
        }

        public void LogMessages(CommandMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            //string output = JsonConvert.SerializeObject(message.Data);
        }

        public void LogMessages(NotificationMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            //string output = JsonConvert.SerializeObject(message.Data);

            //this.inMemoryDataContext.LogEntries.Add(new LogEntry { LogMessage = logMessage });
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

        private async Task ReceiveCommandTaskFunction()
        {
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

                await this.inMemoryDataContext.SaveChangesAsync(this.stoppingToken);
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

                await this.inMemoryDataContext.SaveChangesAsync(this.stoppingToken);
            } while (!this.stoppingToken.IsCancellationRequested);
        }

        #endregion
    }
}
