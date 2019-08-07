using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitStatisticsProvider loadingUnitStatisticsProvider;

        private readonly IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            ILoadingUnitStatisticsProvider loadingUnitStatisticsProvider,
            IMachinesDataService machinesDataService)
        {
            if (loadingUnitStatisticsProvider == null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            }

            if (machinesDataService == null)
            {
                throw new System.ArgumentNullException(nameof(machinesDataService));
            }

            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider;
            this.machinesDataService = machinesDataService;
        }

        #endregion

        #region Methods

        [HttpGet("SpaceStatistics")]
        public async Task<ActionResult<IEnumerable<LoadingUnitSpaceStatistics>>> GetSpaceStatisticsAsync()
        {
            var statistics = this.loadingUnitStatisticsProvider.GetSpaceStatistics();

            try
            {
                var machineId = 1; // TODO this is the WMS machine ID
                var loadingUnits = await this.machinesDataService.GetLoadingUnitsByIdAsync(machineId);
                foreach (var stat in statistics)
                {
                    var loadingUnit = loadingUnits.SingleOrDefault(l => l.Code == stat.Code);
                    if (loadingUnit != null)
                    {
                        stat.CompartmentsCount = loadingUnit.CompartmentsCount;
                        stat.AreaFillPercentage = (decimal?)loadingUnit.AreaFillRate.Value * 100;
                    }
                }
            }
            catch (System.Exception)
            {
                // do nothing: data from WMS will remain to its default values
            }

            return this.Ok(statistics);
        }

        [HttpGet("WeightStatistics")]
        public async Task<ActionResult<IEnumerable<LoadingUnitWeightStatistics>>> GetWeightStatisticsAsync()
        {
            var statistics = this.loadingUnitStatisticsProvider.GetWeightStatistics();
            try
            {
                var machineId = 1; // TODO this is the WMS machine ID
                var loadingUnits = await this.machinesDataService.GetLoadingUnitsByIdAsync(machineId);
                foreach (var stat in statistics)
                {
                    var loadingUnit = loadingUnits.SingleOrDefault(l => l.Code == stat.Code);
                    if (loadingUnit != null)
                    {
                        stat.CompartmentsCount = loadingUnit.CompartmentsCount;
                    }
                }
            }
            catch (System.Exception)
            {
                // do nothing: data from WMS will remain to its default values
            }

            return this.Ok(statistics);
        }

        #endregion
    }
}
