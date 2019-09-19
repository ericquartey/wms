using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellsController : BaseAutomationController
    {
        #region Fields

        private readonly ICellsProvider cellsProvider;

        #endregion

        #region Constructors

        public CellsController(
            ICellsProvider cellsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
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

        [HttpPost("{id}/height")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Cell> UpdateHeight(int id, decimal height)
        {
            try
            {
                var cell = this.cellsProvider.UpdateHeight(id, height);

                return this.Ok(cell);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<Cell>(ex);
            }
        }

        #endregion
    }
}
