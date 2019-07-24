using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Cell;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DataModels.LoadingUnit;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : BackgroundService, IDataLayer
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

        private CancellationToken stoppingToken;

        private bool suppressSecondary;

        #endregion

        #region Constructors

        public DataLayerService(DataLayerConfiguration dataLayerConfiguration, IEventAggregator eventAggregator, ILogger<DataLayerService> logger)
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

            this.dataLayerConfiguration = dataLayerConfiguration;

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

        public void SwitchDBContext()
        {
            var switchContextOptions = this.primaryContextOptions;
            this.primaryContextOptions = this.secondaryContextOptions;
            this.secondaryContextOptions = switchContextOptions;

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
            using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
            {
                primaryDataContext.Database.Migrate();
            }

            using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
            {
                secondaryDataContext.Database.Migrate();
            }

            try
            {
                this.LoadConfigurationValuesInfo(this.dataLayerConfiguration.ConfigurationFilePath);
                //this.EnsureMachinestatusInitialization();
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Exception: {ex.Message} while loading configuration values");
                this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
                return;
            }

            var errorNotification = new NotificationMessage(
                null,
                                                            "DataLayer initialization complete",
                                                            MessageActor.Any,
                                                            MessageActor.DataLayer,
                                                            MessageType.DataLayerReady,
                                                            MessageStatus.NoStatus);
            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(errorNotification);
        }

        private void EnsureMachinestatusInitialization()
        {
            try
            {
                this.GetBoolConfigurationValue((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);
            }
            catch (Exception ex)
            {
                this.SetBoolConfigurationValue((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus, false);
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

                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryDataContext.SaveChanges();
                }

                using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                {
                    secondaryDataContext.SaveChanges();
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

                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    primaryDataContext.SaveChanges();
                }
                using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                {
                    secondaryDataContext.SaveChanges();
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
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

                using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
                {
                    configurationValues = primaryDataContext.ConfigurationValues;

                    cells = primaryDataContext.Cells;

                    freeBlocks = primaryDataContext.FreeBlocks;

                    loadingUnits = primaryDataContext.LoadingUnits;

                    logEntries = primaryDataContext.LogEntries;

                    using (var secondaryDataContext = new DataLayerContext(this.secondaryContextOptions))
                    {
                        if (configurationValues.Any())
                        {
                            secondaryDataContext.ConfigurationValues.UpdateRange(configurationValues);
                            if (secondaryDataContext.ChangeTracker.Entries<ConfigurationValue>().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified))
                            {
                                secondaryDataContext.SaveChanges();
                            }
                        }

                        if (cells.Any())
                        {
                            secondaryDataContext.Cells.UpdateRange(cells);
                            if (secondaryDataContext.ChangeTracker.Entries<Cell>().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified))
                            {
                                secondaryDataContext.SaveChanges();
                            }
                        }

                        if (freeBlocks.Any())
                        {
                            secondaryDataContext.FreeBlocks.UpdateRange(freeBlocks);
                            if (secondaryDataContext.ChangeTracker.Entries<FreeBlock>().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified))
                            {
                                secondaryDataContext.SaveChanges();
                            }
                        }

                        if (loadingUnits.Any())
                        {
                            secondaryDataContext.LoadingUnits.UpdateRange(loadingUnits);
                            if (secondaryDataContext.ChangeTracker.Entries<LoadingUnit>().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified))
                            {
                                secondaryDataContext.SaveChanges();
                            }
                        }

                        if (logEntries.Any())
                        {
                            secondaryDataContext.LogEntries.UpdateRange(logEntries);
                            if (secondaryDataContext.ChangeTracker.Entries<LogEntry>().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified))
                            {
                                secondaryDataContext.SaveChanges();
                            }
                        }
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
