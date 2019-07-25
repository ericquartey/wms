using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Cells;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/[controller]")]
    [ApiController]
    public class CellsController : ControllerBase
    {
        #region Fields

        private readonly ICellManagmentDataLayer dataLayerCellManagment;

        #endregion

        #region Constructors

        public CellsController(ICellManagmentDataLayer dataLayerCellManagment)
        {
            if (dataLayerCellManagment == null)
            {
                throw new ArgumentNullException(nameof(dataLayerCellManagment));
            }

            this.dataLayerCellManagment = dataLayerCellManagment;
        }

        #endregion

        #region Methods

        [HttpGet("Statistics")]
        public ActionResult<CellStatisticsSummary> GetStatistics()
        {
            var statics = this.dataLayerCellManagment.GetCellStatistics();
            return this.Ok(statics);
        }

        #endregion
    }
}
