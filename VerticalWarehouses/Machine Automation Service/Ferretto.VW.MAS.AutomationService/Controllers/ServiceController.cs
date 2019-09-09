using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Microsoft.AspNetCore.Http;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class ServiceController : BaseAutomationController
    {
        #region Fields

        // TODO: avoid hardcoding constants in code
        private const decimal ChainLength = 2850.0M;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ServiceController(
            IEventAggregator eventAggregator,
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            ILogger<ServiceController> logger)
            : base(eventAggregator)
        {
            if (horizontalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalAxisDataLayer));
            }

            if (horizontalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
            this.horizontalAxis = horizontalAxisDataLayer;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
        }

        #endregion

        #region Methods

        [HttpPost("search-horizontal-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SearchHorizontalZero()
        {
            var actualSpeed = this.horizontalAxis.MaxEmptySpeedHA * this.horizontalManualMovements.FeedRateHM;

            decimal[] speed = { actualSpeed };
            decimal[] acceleration = { this.horizontalAxis.MaxEmptyAccelerationHA };
            decimal[] deceleration = { this.horizontalAxis.MaxEmptyDecelerationHA };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.FindZero,
                ChainLength,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition);

            this.PublishCommand(
                    messageData,
                    $"Execute Find Horizontal Zero Positioning Command",
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning);

            return this.Accepted();
        }

        #endregion
    }
}
