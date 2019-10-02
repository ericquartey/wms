using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaysController : BaseAutomationController
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        #endregion

        #region Constructors

        public BaysController(
            IEventAggregator eventAggregator,
            IBaysProvider baysProvider)
            : base(eventAggregator)
        {
            if (baysProvider is null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            this.baysProvider = baysProvider;
        }

        #endregion

        #region Methods

        [HttpPost("{bayNumber}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> ActivateAsync(BayNumber bayNumber)
        {
            var bay = this.baysProvider.Activate(bayNumber);

            return this.Ok(bay);
        }

        [HttpPost("{bayNumber}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> DeactivateAsync(BayNumber bayNumber)
        {
            var bay = this.baysProvider.Deactivate(bayNumber);

            return this.Ok(bay);
        }

        [HttpGet("{bayNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> GetByNumber(BayNumber bayNumber)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);

            return this.Ok(bay);
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult Move(int bayNumber, HorizontalMovementDirection direction)
        {
            throw new NotImplementedException();
        }

        [HttpPost("{bayNumber}/height")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> UpdateHeightAsync(BayNumber bayNumber, int position, double height)
        {
            var bay = this.baysProvider.UpdatePosition(bayNumber, position, height);

            return this.Ok(bay);
        }

        #endregion
    }
}
