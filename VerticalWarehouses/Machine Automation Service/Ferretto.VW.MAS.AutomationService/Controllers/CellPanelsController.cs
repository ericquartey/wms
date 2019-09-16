using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellPanelsController : BaseAutomationController
    {
        #region Fields

        private readonly ICellPanelsProvider cellPanelsProvider;

        #endregion

        #region Constructors

        public CellPanelsController(
            ICellPanelsProvider cellPanelsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            if (cellPanelsProvider is null)
            {
                throw new ArgumentNullException(nameof(cellPanelsProvider));
            }

            this.cellPanelsProvider = cellPanelsProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<CellPanel>> GetAll()
        {
            var panels = this.cellPanelsProvider.GetAll();

            return this.Ok(panels);
        }

        [HttpPost("height")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<CellPanel> UpdateHeight(int cellId, decimal newHeight)
        {
            try
            {
                var panel = this.cellPanelsProvider.UpdateHeight(cellId, newHeight);

                return this.Ok(panel);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<CellPanel>(ex);
            }
        }

        #endregion
    }
}
