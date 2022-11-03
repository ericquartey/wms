using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.SocketLink;
using Ferretto.VW.MAS.SocketLink.Providers;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.Telemetry.Contracts.Hub;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationTelemetryService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly IMachineProvider machineProvider;

        private readonly ITelemetryHubClient telemetryHub;

        private bool enableConnectionFlag;

        #endregion

        #region Constructors

        public NotificationTelemetryService(
            IEventAggregator eventAggregator,
            ITelemetryHubClient telemetryHub,
            IMachineProvider machineProvider,
            ILogger<NotificationTelemetryService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.telemetryHub = telemetryHub ?? throw new ArgumentNullException(nameof(telemetryHub));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));

            this.telemetryHub.MachineReceivedChanged += async (s, e) => await this.TelemetryHub_MachineReceivedChangedAsync(s, e);
            this.telemetryHub.ConnectionStatusChanged += async (s, e) => await this.OnTelemetryHubConnectionStatusChanged(e);
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            this.enableConnectionFlag = false;
            //await this.telemetryHub.ConnectAsync();
        }

        protected override void NotifyCommandError(CommandMessage notificationData)
        {
            if (notificationData is null)
            {
                throw new ArgumentNullException(nameof(notificationData));
            }

            this.Logger.LogDebug($"Notifying Automation Service service error");

            var msg = new NotificationMessage(
                notificationData.Data,
                "AS Error",
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.FsmException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        private static string GetVersion()
        {
            return Assembly
                .GetEntryAssembly()
                .GetName()
                .Version
                .ToString();
        }

        private async Task<string> GetWmsVersionAsync()
        {
            var result = string.Empty;
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var machineWms = scope.ServiceProvider.GetRequiredService<IMachinesWmsWebService>();
                var machineVolatile = scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();

                if (machineVolatile.WmsIsEnabled is true)
                {
                    result = await machineWms.GetWmsVersionAsync(machineVolatile.MachineId.Value);
                }
                else
                {
                    if (machineVolatile.SocketLinkIsEnabled is true)
                    {
                        var socketLinkProvider = scope.ServiceProvider.GetRequiredService<ISocketLinkSyncProvider>();
                        result = $"SL {socketLinkProvider.GetVersion()}";
                    }
                }
            }

            return result;
        }

        private void OnHubConnectionStatusChanged1(object sender, Common.Hubs.ConnectionStatusChangedEventArgs e)
        {
            this.Logger.LogTrace("Connection to Telemetry hub changed (connected={isConnected})", e.IsConnected);
            if (e.IsConnected)
            {
                // To Do
            }
        }

        private async Task OnTelemetryHubConnectionStatusChanged(Common.Hubs.ConnectionStatusChangedEventArgs e)
        {
            await this._semaphore.WaitAsync();

            try
            {
                this.Logger.LogDebug("Connection to Telemetry hub changed (connected={isConnected})", e.IsConnected);

                // When connection to hub client is established, then retrieve the database content
                if (e.IsConnected && !this.enableConnectionFlag)
                {
                    var scope = this.ServiceScopeFactory.CreateScope();
                    var machineDataProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();

                    try
                    {
                        if (machineDataProvider.IsDbSaveOnTelemetry())
                        {
                            // Retrieve the (raw) database content
                            var machine = this.ServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMachineProvider>();
                            var rawDatabaseContent = machine.GetRawDatabaseContent();

                            await this.SendRawDatabaseContentAsync(rawDatabaseContent);
                        }

                        if (machineDataProvider.IsDbSaveOnServer())
                        {
                            var dataLayer = this.ServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDataLayerService>();
                            var machine = machineDataProvider.GetMinMaxHeight();
                            // save the database to server
                            try
                            {
                                dataLayer.CopyMachineDatabaseToServer(machine.BackupServer, machine.BackupServerUsername, machineDataProvider.GetBackupServerPassword(), machineDataProvider.GetSecondaryDatabase(), machine.SerialNumber);
                            }
                            catch (ApplicationException ex)
                            {
                                var errorProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                                errorProvider.RecordNew(DataModels.MachineErrorCode.BackupDatabaseOnServer, additionalText: ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex.Message);
                    }
                }

                this.enableConnectionFlag = e.IsConnected;
            }
            finally
            {
                this._semaphore.Release();
            }
        }

        private async Task TelemetryHub_MachineReceivedChangedAsync(object sender, EventArgs e)
        {
            var machine = this.machineProvider.GetMinMaxHeight();
            var machineDto = new ServiceDesk.Telemetry.Machine
            {
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber,
                Version = NotificationTelemetryService.GetVersion(),
                WmsVersion = await this.GetWmsVersionAsync()
            };

            await this.telemetryHub.SendMachineAsync(machineDto);
        }

        #endregion
    }
}
