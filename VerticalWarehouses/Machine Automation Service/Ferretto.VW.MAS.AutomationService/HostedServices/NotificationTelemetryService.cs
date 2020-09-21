using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.Telemetry.Contracts.Hub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationTelemetryService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IMachineProvider machineProvider;

        private readonly ITelemetryHubClient telemetryHub;

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
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await this.telemetryHub.ConnectAsync();
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

        private void OnHubConnectionStatusChanged1(object sender, Common.Hubs.ConnectionStatusChangedEventArgs e)
        {
            this.Logger.LogTrace("Connection to Telemetry hub changed (connected={isConnected})", e.IsConnected);
            if (e.IsConnected)
            {
                // To Do
            }
        }

        private async Task TelemetryHub_MachineReceivedChangedAsync(object sender, EventArgs e)
        {
            var machine = this.machineProvider.Get();
            var machineDto = new Machine
            {
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber,
                Version = NotificationTelemetryService.GetVersion()
            };

            await this.telemetryHub.SendMachineAsync(machineDto);
        }

        #endregion
    }
}
