using System;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.Cells;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellsController : ControllerBase
    {
        #region Fields

        private readonly ICellsProvider cellsProvider;

        #endregion

        #region Constructors

        public CellsController(ICellsProvider cellsProvider)
        {
            if (cellsProvider == null)
            {
                throw new ArgumentNullException(nameof(cellsProvider));
            }

            this.cellsProvider = cellsProvider;
        }

        #endregion

        #region Methods

        [HttpGet("statistics")]
        public ActionResult<CellStatisticsSummary> GetStatistics()
        {
            var statistics = this.cellsProvider.GetStatistics();

            return this.Ok(statistics);
        }

        #endregion
    }
}
