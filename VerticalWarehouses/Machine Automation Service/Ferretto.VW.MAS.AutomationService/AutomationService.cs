using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
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

        private readonly IBaysDataService baysDataService;

        private readonly IBaysProvider baysProvider;

        private readonly IDataHubClient dataHubClient;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionsDataService missionDataService;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private List<DataModels.Bay> configuredBays;

        private IStateMachine currentStateMachine;

        #endregion

        #region Constructors

        public AutomationService(
            IEventAggregator eventAggregator,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<AutomationService> logger,
            IDataHubClient dataHubClient,
            IMachinesDataService machinesDataService,
            IHubContext<OperatorHub, IOperatorHub> operatorHub,
            IBaysDataService baysDataService,
            IMissionsDataService missionDataService,
            IServiceScopeFactory serviceScopeFactory,
            IApplicationLifetime applicationLifetime,
            IBaysProvider baysProvider)
            : base(eventAggregator, logger)
        {
            this.Logger.LogTrace("1:Method Start");

            this.installationHub = installationHub ?? throw new ArgumentNullException(nameof(installationHub));
            this.dataHubClient = dataHubClient ?? throw new ArgumentNullException(nameof(dataHubClient));
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.operatorHub = operatorHub ?? throw new ArgumentNullException(nameof(operatorHub));
            this.baysDataService = baysDataService ?? throw new ArgumentNullException(nameof(baysDataService));
            this.missionDataService = missionDataService ?? throw new ArgumentNullException(nameof(missionDataService));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
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
