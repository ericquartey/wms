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
    public class MissionsController : ControllerBase
    {
        #region Fields

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionsController(
            IMissionsDataProvider missionsDataProvider)
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

        #endregion
    }
}
