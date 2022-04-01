using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.TimeManagement.Models;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IApplicationLifetime applicationLifetime;

        private readonly IConfiguration configuration;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        #endregion

        #region Constructors

        public NotificationRelayService(
            IEventAggregator eventAggregator,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<NotificationRelayService> logger,
            IHubContext<OperatorHub, IOperatorHub> operatorHub,
            IServiceScopeFactory serviceScopeFactory,
            IApplicationLifetime applicationLifetime,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IConfiguration configuration)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.installationHub = installationHub ?? throw new ArgumentNullException(nameof(installationHub));
            this.operatorHub = operatorHub ?? throw new ArgumentNullException(nameof(operatorHub));
            this.applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));

            this.EventAggregator
                .GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>()
                .Subscribe(async (e) => await this.OnSystemTimeChangedAsync(e), ThreadOption.BackgroundThread, false);
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
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

        #endregion
    }
}
