using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellsController : ControllerBase
    {
        #region Fields

        private readonly ICellsProvider cellsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CellsController(
            ICellsProvider cellsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<Cell>> GetAll()
        {
            var cells = this.cellsProvider.GetAll();

            return this.Ok(cells);
        }

        [HttpGet("height-check-parameters")]
        public ActionResult<PositioningProcedure> GetHeightCheckProcedureParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetCellsHeightCheck());
        }

        [HttpGet("statistics")]
        public ActionResult<CellStatisticsSummary> GetStatistics()
        {
            var statistics = this.cellsProvider.GetStatistics();

            return this.Ok(statistics);
        }

        [HttpPost("save-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult SaveCell(DataModels.Cell cell)
        {
            this.cellsProvider.Save(cell);
            return this.Accepted();
        }

        [HttpPost("save-cells")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult SaveCells(IEnumerable<Cell> cells)
        {
            this.cellsProvider.SaveCells(cells);
            return this.Accepted();
        }

        [HttpPost("{id}/height")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Cell> UpdateHeight(int id, double height)
        {
            var cell = this.cellsProvider.UpdatePosition(id, height);

            return this.Ok(cell);
        }

        [HttpPost("fromid/toid/height")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Cell>> UpdatesHeight(int fromid, int toid, WarehouseSide side, double height)
        {
            var cells = this.cellsProvider.UpdateHeights(fromid, toid, side, height);

            return this.Ok(cells);
        }

        #endregion
    }
}
