using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/[controller]")]
    [ApiController]
    public class CellsController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerCellManagement dataLayerCellManagment;

        #endregion

        #region Constructors

        public CellsController(IDataLayerCellManagement dataLayerCellManagment)
        {
            this.dataLayerCellManagment = dataLayerCellManagment;
        }

        #endregion

        #region Methods

        [HttpGet("Statistics")]
        public ActionResult<CellStatistics> GetStatistics()
        {
            var statics = this.dataLayerCellManagment.GetCellStatistics();
            return this.Ok(statics);
        }

        #endregion
    }
}
