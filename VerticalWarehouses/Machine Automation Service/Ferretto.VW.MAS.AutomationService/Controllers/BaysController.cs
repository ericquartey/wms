using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

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
        public ActionResult<Bay> ActivateAsync(int bayNumber)
        {
            try
            {
                var bay = this.baysProvider.Activate(bayNumber);

                return this.Ok(bay);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<Bay>(ex);
            }
        }

        [HttpPost("{bayNumber}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> DeactivateAsync(int bayNumber)
        {
            try
            {
                var bay = this.baysProvider.Deactivate(bayNumber);

                return this.Ok(bay);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<Bay>(ex);
            }
        }

        [HttpGet("{bayNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> GetByNumber(int bayNumber)
        {
            try
            {
                var bay = this.baysProvider.GetByNumber(bayNumber);

                return this.Ok(bay);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<Bay>(ex);
            }
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
        public ActionResult<Bay> UpdateHeightAsync(int bayNumber, int position, decimal height)
        {
            try
            {
                var bay = this.baysProvider.UpdatePosition(bayNumber, position, height);

                return this.Ok(bay);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<Bay>(ex);
            }
        }

        #endregion
    }
}
