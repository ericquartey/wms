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
    public class CellPanelsController : ControllerBase
    {
        #region Fields

        private readonly ICellPanelsProvider cellPanelsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CellPanelsController(
            ICellPanelsProvider cellPanelsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.cellPanelsProvider = cellPanelsProvider ?? throw new ArgumentNullException(nameof(cellPanelsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<CellPanel>> GetAll()
        {
            var panels = this.cellPanelsProvider.GetAll();

            return this.Ok(panels);
        }

        [HttpGet("height-check-parameters")]
        public ActionResult<PositioningProcedure> GetProcedureParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetCellPanelsCheck());
        }

        [HttpPost("restart")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult RestartProcedure()
        {
            this.cellPanelsProvider.RestartProcedure();

            return this.Accepted();
        }

        [HttpPost("height")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<CellPanel> UpdateHeight(int cellPanelId, double heightDifference)
        {
            var panel = this.cellPanelsProvider.UpdateHeight(cellPanelId, heightDifference);

            return this.Ok(panel);
        }

        #endregion
    }
}
