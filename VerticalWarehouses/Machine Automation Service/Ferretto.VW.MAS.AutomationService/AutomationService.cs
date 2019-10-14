using System;
using System.Linq;
using System.Reflection;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService
{
    public partial class AutomationService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IApplicationLifetime applicationLifetime;

        private readonly IBaysProvider baysProvider;

        private readonly IDataHubClient dataHubClient;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        #endregion

        #region Constructors

        public AutomationService(
            IEventAggregator eventAggregator,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<AutomationService> logger,
            IDataHubClient dataHubClient,
            IHubContext<OperatorHub, IOperatorHub> operatorHub,
            IServiceScopeFactory serviceScopeFactory,
            IApplicationLifetime applicationLifetime,
            IBaysProvider baysProvider)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            if (applicationLifetime is null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            if (installationHub is null)
            {
                throw new ArgumentNullException(nameof(installationHub));
            }

            if (dataHubClient is null)
            {
                throw new ArgumentNullException(nameof(dataHubClient));
            }

            if (operatorHub is null)
            {
                throw new ArgumentNullException(nameof(operatorHub));
            }

            this.Logger.LogTrace("1:Method Start");

            this.installationHub = installationHub;
            this.dataHubClient = dataHubClient;
            this.operatorHub = operatorHub;
            this.applicationLifetime = applicationLifetime;
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this.LogVersion();

            await base.StartAsync(cancellationToken);

            await this.dataHubClient.ConnectAsync();
        }

        protected override void NotifyCommandError(CommandMessage notificationData)
        {
            this.Logger.LogDebug($"Notifying Automation Service service error");

            var msg = new NotificationMessage(
                notificationData.Data,
                "AS Error",
                MessageActor.Any,
                MessageActor.MissionsManager,
                MessageType.FsmException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        protected override void NotifyError(NotificationMessage notificationData)
        {
            throw new NotImplementedException();
        }

        private void LogVersion()
        {
            var versionAttribute = this.GetType().Assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true)
                .FirstOrDefault() as AssemblyInformationalVersionAttribute;

            var versionString = versionAttribute?.InformationalVersion ?? this.GetType().Assembly.GetName().Version.ToString();

            this.Logger.LogInformation($"VertiMag Automation Service version {versionString}");
        }

        #endregion
    }
}
