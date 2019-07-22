using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/[controller]")]
    [ApiController]
    public class LoadingUnitsController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitStatisticsDataLayer loadingUnitStatistics;

        private readonly IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            ILoadingUnitStatisticsDataLayer loadingUnitStatistics,
            IMachinesDataService machinesDataService)
        {
            if (loadingUnitStatistics == null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatistics));
            }

            if (machinesDataService == null)
            {
                throw new System.ArgumentNullException(nameof(machinesDataService));
            }

            this.loadingUnitStatistics = loadingUnitStatistics;
            this.machinesDataService = machinesDataService;
        }

        #endregion

        #region Methods

        [HttpGet("SpaceStatistics")]
        public async Task<ActionResult<IEnumerable<LoadingUnitSpaceStatistics>>> GetSpaceStatisticsAsync()
        {
            var statistics = this.loadingUnitStatistics.GetSpaceStatistics();

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
            var statistics = this.loadingUnitStatistics.GetWeightStatistics();
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
