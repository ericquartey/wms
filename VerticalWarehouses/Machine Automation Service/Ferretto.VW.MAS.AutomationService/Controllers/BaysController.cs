using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaysController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public BaysController(
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IBaysDataProvider baysDataProvider)
        {
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
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
            var bay = this.baysDataProvider.Activate(this.BayNumber);

            return this.Ok(bay);
        }

        [HttpPost("deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> DeactivateAsync()
        {
            var bay = this.baysDataProvider.Deactivate(this.BayNumber);

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

        [HttpGet("height-check-parameters")]
        public ActionResult<PositioningProcedure> GetHeightCheckParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetBayHeightCheck());
        }

        [HttpPost("homing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Homing()
        {
            this.baysDataProvider.PerformHoming(this.BayNumber);

            return this.Accepted();
        }

        [HttpPost("light")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Light(bool enable)
        {
            this.baysDataProvider.Light(this.BayNumber, enable);

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
