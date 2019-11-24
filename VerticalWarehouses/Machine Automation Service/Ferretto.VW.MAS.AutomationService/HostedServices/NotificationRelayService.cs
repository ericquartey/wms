using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationRelayService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IApplicationLifetime applicationLifetime;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IConfiguration configuration;

        private readonly IDataHubClient dataHubClient;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        #endregion

        #region Constructors

        public NotificationRelayService(
            IEventAggregator eventAggregator,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<NotificationRelayService> logger,
            IDataHubClient dataHubClient,
            IHubContext<OperatorHub, IOperatorHub> operatorHub,
            IServiceScopeFactory serviceScopeFactory,
            IApplicationLifetime applicationLifetime,
            IBaysDataProvider baysDataProvider,
            IConfiguration configuration)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.installationHub = installationHub ?? throw new ArgumentNullException(nameof(installationHub));
            this.dataHubClient = dataHubClient ?? throw new ArgumentNullException(nameof(dataHubClient));
            this.operatorHub = operatorHub ?? throw new ArgumentNullException(nameof(operatorHub));
            this.applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.dataHubClient.EntityChanged += this.OnDataHubClientEntityChanged;
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            if (this.configuration.IsWmsEnabled())
            {
                await this.dataHubClient.ConnectAsync();
            }
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

        private void OnDataHubClientEntityChanged(object sender, EntityChangedEventArgs e)
        {
            if (e.Operation != WMS.Data.Hubs.Models.HubEntityOperation.Created)
            {
                return;
            }

            switch (e.EntityType)
            {
                case nameof(MissionOperation):
                    {
                        var msg = new NotificationMessage(
                            null,
                            "New WMS mission available",
                            MessageActor.Any,
                            MessageActor.AutomationService,
                            MessageType.NewWmsMissionAvailable,
                            BayNumber.None,
                            BayNumber.None,
                            MessageStatus.OperationStart);

                        this.EventAggregator
                            .GetEvent<NotificationEvent>()
                            .Publish(msg);
                        break;
                    }
            }
        }

        #endregion
    }
}
