using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

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

        [HttpPost("{id}/height")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Cell> UpdateHeight(int id, double height)
        {
            var cell = this.cellsProvider.UpdateHeight(id, height);

            return this.Ok(cell);
        }

        #endregion
    }
}
