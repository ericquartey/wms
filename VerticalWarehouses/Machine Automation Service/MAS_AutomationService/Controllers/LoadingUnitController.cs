using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/[controller]")]
    [ApiController]
    public class LoadingUnitsController : ControllerBase
    {
        #region Fields

        public ILoadingUnitStatistics loadingUnitStatistics;

        #endregion

        #region Constructors

        public LoadingUnitsController(ILoadingUnitStatistics loadingUnitStatistics)
        {
            this.loadingUnitStatistics = loadingUnitStatistics;
        }

        #endregion

        #region Methods

        [HttpGet("Dimension")]
        public ActionResult<IEnumerable<LoadingUnitSpaceStatistics>> GetDimension()
        {
            var statistics = this.loadingUnitStatistics.GetWeightStatistics();
            return this.Ok(statistics);
        }

        [HttpGet("Space-Statistics")]
        public ActionResult<IEnumerable<LoadingUnitSpaceStatistics>> GetSpaceStatistics()
        {
            var statistics = this.loadingUnitStatistics.GetSpaceStatistics();
            return this.Ok(statistics);
        }

        [HttpGet("Weight-Statistics")]
        public ActionResult<IEnumerable<LoadingUnitWeightStatistics>> GetWeightStatistics()
        {
            var statistics = this.loadingUnitStatistics.GetWeightStatistics();
            return this.Ok(statistics);
        }

        #endregion
    }
}
