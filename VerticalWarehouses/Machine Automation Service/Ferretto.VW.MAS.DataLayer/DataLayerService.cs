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
    public partial class DataLayerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>, IDataLayerService
    {
        #region Constructors

        public DataLayerService(
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
        }

        #endregion

        #region Properties

        public bool IsReady { get; private set; }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await this.InitializeAsync();
        }

        protected override bool FilterCommand(CommandMessage command)
        {
            return true;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return true;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<ILogEntriesProvider>().Add(command);

            return Task.CompletedTask;
        }

        protected override Task OnNotificationReceivedAsync(NotificationMessage notification, IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<ILogEntriesProvider>().Add(notification);

            return Task.CompletedTask;
        }

        private async Task ApplyMigrationsAsync()
        {
            try
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var redundancyService = scope
                        .ServiceProvider
                        .GetRequiredService<IDbContextRedundancyService<DataLayerContext>>();

                    redundancyService.IsEnabled = false;

                    using (var activeDbContext = new DataLayerContext(redundancyService.ActiveDbContextOptions))
                    {
                        var pendingMigrations = await activeDbContext.Database.GetPendingMigrationsAsync();
                        if (pendingMigrations.Any())
                        {
                            this.Logger.LogInformation($"Applying {pendingMigrations.Count()} migrations to active database ...");
                            await activeDbContext.Database.MigrateAsync();
                        }
                    }

                    using (var standbyDbContext = new DataLayerContext(redundancyService.StandbyDbContextOptions))
                    {
                        var pendingMigrations = await standbyDbContext.Database.GetPendingMigrationsAsync();
                        if (pendingMigrations.Any())
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

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var loadingUnitsProvider = scope.ServiceProvider.GetRequiredService<ILoadingUnitsProvider>();

                try
                {
                    this.LoadConfigurationValuesInfo(configuration.GetDataLayerConfigurationFile());

                    await loadingUnitsProvider.LoadFromAsync(configuration.GetLoadingUnitsConfigurationFile());

                    this.IsReady = true;

                    var message = new NotificationMessage(
                        null,
                        "DataLayer initialization complete.",
                        MessageActor.Any,
                        MessageActor.DataLayer,
                        MessageType.DataLayerReady,
                        BayNumber.None);

                    this.EventAggregator
                        .GetEvent<NotificationEvent>()
                        .Publish(message);

                    this.Logger.LogDebug("Data layer service initialized.");
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while loading configuration values.");
                    this.SendErrorMessage(new DLExceptionMessageData(ex));
                }
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
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(message);
        }

        #endregion
    }
}
