using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed partial class WmsDataSyncService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IDataLayerService dataLayerService;

        private readonly ILoadingUnitsDataService loadingUnitsDataService;

        private readonly IMachinesDataService machineDataService;

        private bool initialSyncPerformed;

        #endregion

        #region Constructors

        public WmsDataSyncService(
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            ILogger<WmsDataSyncService> logger,
            IDataLayerService dataLayerService,
            ILoadingUnitsDataService loadingUnitsDataService,
            IMachinesDataService machineDataService,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataLayerService));

            this.loadingUnitsDataService = loadingUnitsDataService ?? throw new ArgumentNullException(nameof(dataLayerService));
            this.machineDataService = machineDataService ?? throw new ArgumentNullException(nameof(dataLayerService));
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            if (this.dataLayerService.IsReady)
            {
                await this.OnDataLayerReadyAsync();
            }
        }

        private async Task AlignLoadingUnitsAsync()
        {
            try
            {
                this.Logger.LogDebug("Syncing loading unit catalog with WMS ...");

                int machineId;
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    machineId = scope.ServiceProvider
                        .GetRequiredService<IMachineProvider>()
                        .GetIdentity();
                }

                // 1. Ask WMS for all loading units in this machine
                var wmsLoadingUnits = await this.machineDataService.GetLoadingUnitsByIdAsync(machineId);
                var wmsIds = wmsLoadingUnits.Select(l => l.Id);

                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var loadingUnitsDataProvider = scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();

                    // 2. Get all loading units configured in this machine
                    var localLoadingUnits = loadingUnitsDataProvider.GetAll();

                    // 3. Identify missing loading units in WMS and send them
                    var missingWmsLoadingUnits = localLoadingUnits.Where(l => wmsLoadingUnits.All(extLu => extLu.Id != l.Id));
                    foreach (var loadingUnit in missingWmsLoadingUnits)
                    {
                        this.Logger.LogDebug($"Pushing new loading unit {loadingUnit.Id} to WMS.");
                        await this.loadingUnitsDataService.CreateAsync(
                            new LoadingUnitDetails
                            {
                                Id = loadingUnit.Id,
                                MachineId = machineId,
                                Code = "n/a",
                                CreationDate = DateTime.Now
                            });

                        var createdLoadingUnit = await this.loadingUnitsDataService.GetByIdAsync(loadingUnit.Id);

                        loadingUnitsDataProvider.SetCode(loadingUnit.Id, createdLoadingUnit.Code);
                    }

                    // 4. Identify missing loading units in machine and delete them from WMS
                    var missingLocalLoadingUnits = wmsLoadingUnits.Where(l => localLoadingUnits.All(localLu => localLu.Id != l.Id));
                    foreach (var loadingUnit in missingLocalLoadingUnits)
                    {
                        this.Logger.LogDebug($"Removing loading unit {loadingUnit.Id} from WMS.");
                        await this.loadingUnitsDataService.DeleteAsync(loadingUnit.Id);
                    }
                }

                this.Logger.LogDebug("Loading unit catalog aligned with WMS.");
                this.initialSyncPerformed = true;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Unable to align loading units with WMS.");
            }
        }

        private async Task OnDataLayerReadyAsync()
        {
            if (this.configuration.IsWmsEnabled())
            {
                await this.AlignLoadingUnitsAsync();
            }
        }

        #endregion
    }
}
