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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : BackgroundService, IDataLayer, IWriteLogService
    {
        #region Fields

        private readonly Task commadReceiveTask;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly IEventAggregator eventAggregator;

        private readonly IOptions<FilesInfo> filesInfo;

        private readonly DataLayerContext inMemoryDataContext;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public DataLayer(string connectionString, DataLayerContext inMemoryDataContext, IEventAggregator eventAggregator, IOptions<FilesInfo> filesInfo)
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

            using (var initialContext = new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options))
            {
                initialContext.Database.Migrate();

                if (!initialContext.ConfigurationValues.Any())
                {
                    //TODO reovery database from permanent storage
                }

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

            this.SetDecimalConfigurationValue(ConfigurationValueEnum.CellSpacing, 25.01m);

            this.GetDecimalConfigurationValue(ConfigurationValueEnum.CellSpacing);
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

                ConfigurationValueEnum jsonElementName;
                DataTypeEnum jsonElementType;

                foreach (var jsonElement in jsonObject)
                {
                    jsonElementName = (ConfigurationValueEnum)Enum.Parse(typeof(ConfigurationValueEnum), jsonElement.Key);
                    jsonElementType = this.ConvertConfigurationValue(jsonElementName);

                    switch (jsonElementType)
                    {
                        case (DataTypeEnum.booleanType):
                            {
                                this.SetBoolConfigurationValue(jsonElementName, (bool)jsonElement.Value.ToObject(typeof(bool)));
                                break;
                            }
                        case (DataTypeEnum.dateTimeType):
                            {
                                this.SetDateTimeConfigurationValue(jsonElementName, (DateTime)jsonElement.Value.ToObject(typeof(DateTime)));
                                break;
                            }
                        case (DataTypeEnum.decimalType):
                            {
                                this.SetDecimalConfigurationValue(jsonElementName, (decimal)jsonElement.Value.ToObject(typeof(decimal)));
                                break;
                            }
                        case (DataTypeEnum.integerType):
                            {
                                this.SetIntegerConfigurationValue(jsonElementName, (int)jsonElement.Value.ToObject(typeof(int)));
                                break;
                            }
                        case (DataTypeEnum.stringType):
                            {
                                this.SetStringConfigurationValue(jsonElementName, jsonElement.Value.ToString());
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

        public bool LogWriting(string logMessage)
        {
            var updateOperation = true;

            try
            {
                this.inMemoryDataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
                this.inMemoryDataContext.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                updateOperation = false;
            }

            return updateOperation;
        }

        public void LogWriting(CommandMessage command_EventParameter)
        {
            string logMessage;

            switch (command_EventParameter.Type)
            {
                case MessageType.Homing:
                    {
                        logMessage = "Vertical Homing";
                        break;
                    }
                default:
                    {
                        logMessage = "Unknown Action";

                        break;
                    }
            }

            this.inMemoryDataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
            this.inMemoryDataContext.SaveChanges();
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

                this.LogWriting(receivedMessage);

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
