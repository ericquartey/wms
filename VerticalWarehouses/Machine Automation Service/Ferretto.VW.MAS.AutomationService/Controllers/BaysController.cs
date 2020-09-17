using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaysController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILogger<BaysController> logger;

        #endregion

        #region Constructors

        public BaysController(IBaysDataProvider baysDataProvider,
            ILogger<BaysController> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> ActivateAsync()
        {
            var bay = this.baysDataProvider.SetBayActive(this.BayNumber, active: true);

            return this.Ok(bay);
        }

        [HttpPost("deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> DeactivateAsync()
        {
            var bay = this.baysDataProvider.SetBayActive(this.BayNumber, active: false);

            return this.Ok(bay);
        }

        [HttpPost("find-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            this.baysDataProvider.FindZero(this.BayNumber);

            return this.Accepted();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<Bay>> GetAll()
        {
            var bay = this.baysDataProvider.GetAll();

            return this.Ok(bay);
        }

        [HttpGet("{bayNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> GetByNumber(BayNumber bayNumber)
        {
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            return this.Ok(bay);
        }

        [HttpPost("get-light")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<bool> GetLight()
        {
            return this.Ok(this.baysDataProvider.GetLightOn(this.BayNumber));
        }

        [HttpPost("homing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Homing()
        {
            this.baysDataProvider.PerformHoming(this.BayNumber);

            return this.Accepted();
        }

        [HttpGet("remove-load-unit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult RemoveLoadUnit(int loadingUnitId)
        {
            this.logger.LogInformation($"Remove load unit {loadingUnitId} by UI");
            this.baysDataProvider.RemoveLoadingUnit(loadingUnitId);
            return this.Accepted();
        }

        [HttpPost("set-all-OpertionBay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetAllOpertionBay(bool pick, bool put, bool view, bool inventory, int bayid)
        {
            this.baysDataProvider.SetAllOpertionBay(pick, put, view, inventory, bayid);
            return this.Accepted();
        }

        [HttpPost("set-light")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetLight(bool enable)
        {
            this.baysDataProvider.Light(this.BayNumber, enable);

            return this.Accepted();
        }

        [HttpPost("set-loadunit-on-elevator")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetLoadUnitOnBay(int bayPositionId, int loadingUnitId)
        {
            this.baysDataProvider.SetLoadingUnit(bayPositionId, loadingUnitId);
            return this.Accepted();
        }

        [HttpPost("height")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> UpdateHeightAsync(int position, double height)
        {
            var bay = this.baysDataProvider.UpdatePosition(this.BayNumber, position, height);

            return this.Ok(bay);
        }

        #endregion
    }
}
