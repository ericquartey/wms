using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
            if (cellsProvider is null)
            {
                throw new ArgumentNullException(nameof(cellsProvider));
            }

            this.cellsProvider = cellsProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<Cell>> GetAll()
        {
            var cells = this.cellsProvider.GetAll();

            return this.Ok(cells);
        }

        [HttpGet("statistics")]
        public ActionResult<CellStatisticsSummary> GetStatistics()
        {
            var statistics = this.cellsProvider.GetStatistics();

            return this.Ok(statistics);
        }

        [HttpPost("height")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Cell> UpdateHeight(int cellId, decimal height)
        {
            try
            {
                var cell = this.cellsProvider.UpdateHeight(cellId, height);

                return this.Ok(cell);
            }
            catch (DataLayer.Exceptions.EntityNotFoundException ex)
            {
                return this.NotFound(new ProblemDetails { Detail = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return this.BadRequest(new ProblemDetails { Detail = ex.Message });
            }
        }

        #endregion
    }
}
