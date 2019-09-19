using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService
{
    internal partial class AutomationService : AutomationBackgroundService
    {
        #region Fields

        private readonly IApplicationLifetime applicationLifetime;

        private readonly IBaysDataService baysDataService;

        private readonly IDataHubClient dataHubClient;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionsDataService missionDataService;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        private readonly IServiceScopeFactory serviceScopeFactory;

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
            IApplicationLifetime applicationLifetime)
            : base(eventAggregator, logger)
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

            if (machinesDataService is null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            if (operatorHub is null)
            {
                throw new ArgumentNullException(nameof(operatorHub));
            }

            if (baysDataService is null)
            {
                throw new ArgumentNullException(nameof(baysDataService));
            }

            if (missionDataService is null)
            {
                throw new ArgumentNullException(nameof(missionDataService));
            }

            this.Logger.LogTrace("1:Method Start");

            this.installationHub = installationHub;
            this.dataHubClient = dataHubClient;
            this.machinesDataService = machinesDataService;
            this.operatorHub = operatorHub;
            this.baysDataService = baysDataService;
            this.missionDataService = missionDataService;
            this.serviceScopeFactory = serviceScopeFactory;
            this.applicationLifetime = applicationLifetime;
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await this.dataHubClient.ConnectAsync();
        }

        #endregion
    }
}
