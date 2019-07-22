using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/[controller]")]
    [ApiController]
    public class LoadingUnitsController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitStatistics loadingUnitStatistics;

        private readonly IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            ILoadingUnitStatistics loadingUnitStatistics,
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
        public ActionResult<IEnumerable<LoadingUnitSpaceStatistics>> GetSpaceStatistics()
        {
            var statistics = this.loadingUnitStatistics.GetSpaceStatistics();
            try
            {
                var machineId = 1; // TODO this is the WMS machine ID
                // TODO need to update WMS service data contracts
                //var wmsLoadingUnits = this.machinesDataService.GetAllLoadingUnitsById(machineId);
                foreach (var stat in statistics)
                {
                    //stat.CompartmentsCount = wmsLoadingUnits.SingleOrDefault(l => l.Code == stat.Code)?.CompartmentsCount;
                    //stat.AreaFillPercentage = wmsLoadingUnits.SingleOrDefault(l => l.Code == stat.Code)?.areaFillRate * 100;
                }
            }
            catch (System.Exception)
            {
                // DO nothing
            }
            return this.Ok(statistics);
        }

        [HttpGet("WeightStatistics")]
        public ActionResult<IEnumerable<LoadingUnitWeightStatistics>> GetWeightStatistics()
        {
            var statistics = this.loadingUnitStatistics.GetWeightStatistics();
            try
            {
                var machineId = 1; // TODO this is the WMS machine ID
                // TODO need to update WMS service data contracts
                //var wmsLoadingUnits = this.machinesDataService.GetAllLoadingUnitsById(machineId);
                foreach (var stat in statistics)
                {
                    //stat.CompartmentsCount = wmsLoadingUnits.SingleOrDefault(l => l.Code == stat.Code)?.CompartmentsCount;
                }
            }
            catch (System.Exception)
            {
                // DO nothing
            }
            return this.Ok(statistics);
        }

        #endregion
    }
}
