using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellsController : BaseAutomationController
    {
        #region Fields

        private readonly ICellsProvider cellsProvider;

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        #endregion

        #region Constructors

        public CellsController(
            ICellsProvider cellsProvider,
            IConfigurationValueManagmentDataLayer configurationProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            if (cellsProvider is null)
            {
                throw new ArgumentNullException(nameof(cellsProvider));
            }

            if (configurationProvider is null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            this.cellsProvider = cellsProvider;
            this.configurationProvider = configurationProvider;
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
            catch (Exception ex)
            {
                return this.NegativeResponse<Cell>(ex);
            }
        }

        #endregion
    }
}
