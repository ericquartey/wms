﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarouselController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly ICarouselProvider carouselProvider;

        #endregion

        #region Constructors

        public CarouselController(ICarouselProvider carouselProvider)
        {
            this.carouselProvider = carouselProvider ?? throw new ArgumentNullException(nameof(carouselProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("can-move")]
        public ActionResult<ActionPolicy> CanMove(VerticalMovementDirection direction)
        {
            return this.Ok(this.carouselProvider.CanMove(direction, this.BayNumber));
        }

        [HttpPost("find-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            this.carouselProvider.Homing(Calibration.FindSensor, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpGet("position")]
        public ActionResult<double> GetPosition()
        {
            return this.Ok(this.carouselProvider.GetPosition(this.BayNumber));
        }

        [HttpPost("homing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Homing()
        {
            this.carouselProvider.Homing(Calibration.ResetEncoder, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult Move(VerticalMovementDirection direction)
        {
            this.carouselProvider.Move(direction, null, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-assisted")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveAssisted(VerticalMovementDirection direction)
        {
            this.carouselProvider.MoveAssisted(direction, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveManual(VerticalMovementDirection direction)
        {
            this.carouselProvider.MoveManual(direction, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.carouselProvider.Stop(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        #endregion
    }
}
