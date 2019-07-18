//Header test C#
using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        private readonly SetupStatusVolatile setupStatusVolatile;

        private DataLayerContext primaryDataContext;

        private DataLayerContext secondaryDataContext;

        private CancellationToken stoppingToken;

        private bool suppressSecondary;

        #endregion

        #region Constructors

        public DataLayer(
            DataLayerConfiguration dataLayerConfiguration,
            DataLayerContext primaryDataContext,
            IEventAggregator eventAggregator,
            ILogger<DataLayer> logger)
        {
            if (primaryDataContext == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(), string.Empty, 0));
            }

            if (eventAggregator == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(), string.Empty, 0));
            }

            if (logger == null)
            {
                this.SendMessage(new DLExceptionMessageData(new ArgumentNullException(), string.Empty, 0));
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

            //var commandLogEvent = this.eventAggregator.GetEvent<CommandEvent>();
            //commandLogEvent.Subscribe(
            //    commandMessage => { this.commandLogQueue.Enqueue(commandMessage); },
            //    ThreadOption.PublisherThread,
            //    false);

            //var notificationLogEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            //notificationLogEvent.Subscribe(
            //    notificationMessage => { this.notificationLogQueue.Enqueue(notificationMessage); },
            //    ThreadOption.PublisherThread,
            //    false);

            this.logger.LogInformation("DataLayer Constructor");
        }

        #endregion

        #region Methods

        public void SwitchDBContext()
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
                this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
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

            // TEMP catch (DataLayerException ex)
            catch (Exception ex)
            {
                this.logger.LogError($"Exception: {ex.Message} while loading configuration values");
                this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
            }

            await this.SecondaryDataLayerInitializeAsync();

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

                    case MessageType.MissionAdded:
                        break;

                    case MessageType.CreateMission:
                        break;

                    case MessageType.Positioning:
                        break;
                }

                await this.primaryDataContext.SaveChangesAsync(this.stoppingToken);
            }
            while (!this.stoppingToken.IsCancellationRequested);
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

                    case MessageType.MissionAdded:
                        break;

                    case MessageType.CreateMission:
                        break;

                    case MessageType.Positioning:
                        break;
                }

                await this.primaryDataContext.SaveChangesAsync(this.stoppingToken);
            }
            while (!this.stoppingToken.IsCancellationRequested);
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
                    this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
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
