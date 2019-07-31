using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : AutomationBackgroundService, IDataLayerService
    {
        #region Fields

        private readonly Task applicationLogWriteTask;

        private readonly BlockingConcurrentQueue<CommandMessage> commandLogQueue = new BlockingConcurrentQueue<CommandMessage>();

        private readonly DataLayerConfiguration dataLayerConfiguration;

        private readonly IEventAggregator eventAggregator;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationLogQueue = new BlockingConcurrentQueue<NotificationMessage>();

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly SetupStatusVolatile setupStatusVolatile = new SetupStatusVolatile();

        #endregion

        #region Constructors

        public DataLayerService(
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DataLayerService> logger)
            : base(eventAggregator, logger)
        {
            if (serviceScopeFactory == null)
            {
                this.SendErrorMessage(new DLExceptionMessageData(new ArgumentNullException(nameof(serviceScopeFactory))));
            }

            this.serviceScopeFactory = serviceScopeFactory;
            this.applicationLogWriteTask = new Task(this.ApplicationLogWriterTaskFunction);

            this.Logger.LogInformation("DataLayer service initialised.");
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this.DataLayerInitialize();

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken);

            try
            {
                this.applicationLogWriteTask.Start();
            }
            catch (Exception ex)
            {
                this.SendErrorMessage(new DLExceptionMessageData(ex, ex.Message, 0));
            }
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command)
        {
            this.CommitDbChanges();

            return Task.CompletedTask;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message)
        {
            this.CommitDbChanges();

            return Task.CompletedTask;
        }

        private void CommitDbChanges()
        {
            try
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    scope.ServiceProvider
                        .GetRequiredService<DataLayerContext>()
                        .SaveChanges();
                }
            }
            catch (Exception ex)
            {
                this.SendErrorMessage(new DLExceptionMessageData(ex));
            }
        }

        private void DataLayerInitialize()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var redundancyService = scope.ServiceProvider
                    .GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

                try
                {
                    using (var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions))
                    {
                        activeDbContext.Database.Migrate();
                    }

                    using (var standbyDbContext = new DataLayerContext(redundancyService.StandbyDbContextOptions))
                    {
                        standbyDbContext.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError($"Exception: {ex.Message} while migating databases.");
                    this.SendErrorMessage(new DLExceptionMessageData(ex));
                    return;
                }
            }

            try
            {
                this.LoadConfigurationValuesInfo(this.dataLayerConfiguration.ConfigurationFilePath);
                //this.EnsureMachinestatusInitialization();
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Exception: {ex.Message} while loading configuration values");
                this.SendErrorMessage(new DLExceptionMessageData(ex));
                return;
            }

            var errorNotification = new NotificationMessage(
                null,
                "DataLayer initialization complete",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DataLayerReady,
                MessageStatus.NoStatus);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(errorNotification);
        }

        private void EnsureMachinestatusInitialization()
        {
            try
            {
                this.GetBoolConfigurationValue((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);
            }
            catch (Exception)
            {
                this.SetBoolConfigurationValue((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus, false);
            }
        }

        private void SendErrorMessage(IMessageData data)
        {
            var msg = new NotificationMessage(
                data,
                "DataLayer Error",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DLException,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(msg);
        }

        #endregion
    }
}
