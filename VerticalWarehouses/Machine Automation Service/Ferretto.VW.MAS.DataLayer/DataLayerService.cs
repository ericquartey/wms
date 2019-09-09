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
            await this.InitializeAsync();

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
                BayIndex = command.BayIndex.ToString(),
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
                BayIndex = message.BayIndex.ToString(),
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

        private async Task ApplyMigrationsAsync()
        {
            try
            {
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var redundancyService = scope.ServiceProvider
                        .GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

                    redundancyService.IsEnabled = false;

                    using (var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions))
                    {
                        var pendingMigrations = await activeDbContext.Database.GetPendingMigrationsAsync();
                        if (pendingMigrations.Count() > 0)
                        {
                            this.Logger.LogInformation($"Applying {pendingMigrations.Count()} migrations to active database ...");
                            await activeDbContext.Database.MigrateAsync();
                        }
                    }

                    using (var standbyDbContext = new DataLayerContext(redundancyService.StandbyDbContextOptions))
                    {
                        var pendingMigrations = await standbyDbContext.Database.GetPendingMigrationsAsync();
                        if (pendingMigrations.Count() > 0)
                        {
                            this.Logger.LogInformation($"Applying {pendingMigrations.Count()} migrations to standby database ...");
                            await standbyDbContext.Database.MigrateAsync();
                        }
                    }

                    redundancyService.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while migating databases.");
                this.SendErrorMessage(new DLExceptionMessageData(ex));
            }
        }

        private async Task InitializeAsync()
        {
            await this.ApplyMigrationsAsync();

            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                try
                {
                    this.LoadConfigurationValuesInfo(configuration.GetDataLayerConfigurationFile());

                    scope.ServiceProvider
                        .GetRequiredService<ICellsProvider>()
                        .LoadFrom(configuration.GetCellsConfigurationFile());

                    await scope.ServiceProvider
                       .GetRequiredService<ILoadingUnitsProvider>()
                       .LoadFromAsync(configuration.GetLoadingUnitsConfigurationFile());
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while loading configuration values.");
                    this.SendErrorMessage(new DLExceptionMessageData(ex));
                    return;
                }
            }

            this.IsReady = true;

            var message = new NotificationMessage(
                null,
                "DataLayer initialization complete.",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DataLayerReady,
                BayIndex.None);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(message);

            this.Logger.LogDebug("Data layer service initialized.");
        }

        private void SendErrorMessage(IMessageData data)
        {
            var message = new NotificationMessage(
                data,
                "DataLayer Error",
                MessageActor.Any,
                MessageActor.DataLayer,
                MessageType.DlException,
                BayIndex.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(message);
        }

        #endregion
    }
}
