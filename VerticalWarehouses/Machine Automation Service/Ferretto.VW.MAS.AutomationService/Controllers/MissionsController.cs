using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        #region Fields

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionsController(IMissionsDataProvider missionsDataProvider)
        {
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<Mission>> GetAll()
        {
            var missions = this.missionsDataProvider.GetAllActiveMissions();

            return this.Ok(missions);
        }

        [HttpGet("{id}/wms")]
        public async Task<ActionResult<WMS.Data.WebAPI.Contracts.MissionInfo>> GetByWmsIdAsync(int id, [FromServices] WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService)
        {
            if (missionsWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(missionsWmsWebService));
            }

            return this.Ok(await missionsWmsWebService.GetByIdAsync(id));
        }

        [HttpGet("{id}/wms/details")]
        public async Task<ActionResult<WMS.Data.WebAPI.Contracts.MissionWithLoadingUnitDetails>> GetWmsDetailsByIdAsync(
            int id,
            [FromServices] WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService)
        {
            if (missionsWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(missionsWmsWebService));
            }

            return this.Ok(await missionsWmsWebService.GetDetailsByIdAsync(id));
        }

        #endregion
    }
}
