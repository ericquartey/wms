using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Extensions;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        private readonly IServiceScopeFactory serviceScopeFactory;

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
                this.SendErrorMessage(
                    new DLExceptionMessageData(
                        new ArgumentNullException(nameof(serviceScopeFactory))));
            }

            this.serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public bool IsReady { get; private set; }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this.DataLayerInitialize();

            await base.StartAsync(cancellationToken);
        }

        protected override bool FilterCommand(CommandMessage command)
        {
            return true;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return true;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command)
        {
            this.Logger.LogTrace($"2:command={command}");

            var serializedData = SerializeMessageData(command.Data);

            var logEntry = new LogEntry
            {
                Data = serializedData,
                Description = command.Description,
                Destination = command.Destination.ToString(),
                Source = command.Source.ToString(),
                TimeStamp = DateTime.UtcNow,
                Type = command.Type.ToString(),
            };

            this.SaveEntryToDb(logEntry);

            return Task.CompletedTask;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage message)
        {
            this.Logger.LogTrace($"2:message={message}");

            var serializedData = SerializeMessageData(message.Data);

            var logEntry = new LogEntry
            {
                Data = serializedData,
                Description = message.Description,
                Destination = message.Destination.ToString(),
                Source = message.Source.ToString(),
                TimeStamp = DateTime.UtcNow,
                Type = message.Type.ToString(),
                ErrorLevel = message.ErrorLevel.ToString(),
                Status = message.Status.ToString(),
            };

            this.SaveEntryToDb(logEntry);

            return Task.CompletedTask;
        }

        private void DataLayerInitialize()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                try
                {
                    var redundancyService = scope.ServiceProvider
                        .GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

                    redundancyService.IsEnabled = false;

                    using (var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions))
                    {
                        var pendingMigrationsCount = activeDbContext.Database.GetPendingMigrations().Count();
                        if (pendingMigrationsCount > 0)
                        {
                            this.Logger.LogInformation($"Applying {pendingMigrationsCount} migrations to active database ...");
                            activeDbContext.Database.Migrate();
                        }
                    }

                    using (var standbyDbContext = new DataLayerContext(redundancyService.StandbyDbContextOptions))
                    {
                        var pendingMigrationsCount = standbyDbContext.Database.GetPendingMigrations().Count();
                        if (pendingMigrationsCount > 0)
                        {
                            this.Logger.LogInformation($"Applying {pendingMigrationsCount} migrations to standby database ...");
                            standbyDbContext.Database.Migrate();
                        }
                    }

                    redundancyService.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while migating databases.");
                    this.SendErrorMessage(new DLExceptionMessageData(ex));
                    return;
                }

                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                try
                {
                    this.LoadConfigurationValuesInfo(configuration.GetDataLayerConfigurationFile());
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while loading configuration values.");
                    this.SendErrorMessage(new DLExceptionMessageData(ex));
                    return;
                }

                try
                {
                    this.Logger.LogInformation("Loading cells from configuration file ...");

                    scope.ServiceProvider
                        .GetRequiredService<ICellsProvider>()
                        .LoadFrom(configuration.GetCellsConfigurationFile());
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while loading cells.");
                    this.SendErrorMessage(new DLExceptionMessageData(ex));
                    return;
                }

                this.IsReady = true;

                var message = new NotificationMessage(
                    null,
                    "DataLayer initialization complete.",
                    MessageActor.Any,
                    MessageActor.DataLayer,
                    MessageType.DataLayerReady);

                this.EventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(message);

                this.Logger.LogDebug("Data layer service initialized.");
            }
        }

        private void SendErrorMessage(IMessageData data)
        {
            var message = new NotificationMessage(
                data,
                "DataLayer Error",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DlException,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(message);
        }

        #endregion
    }
}
