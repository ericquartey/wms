using System;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaysController : ControllerBase
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        #endregion

        #region Constructors

        public BaysController(IBaysProvider baysProvider)
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
            var bay = this.baysProvider.Activate(bayNumber);
            if (bay is null)
            {
                return this.NotFound();
            }

            return this.Ok(bay);
        }

        [HttpPost("{bayNumber}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> DeactivateAsync(int bayNumber)
        {
            var bay = this.baysProvider.Deactivate(bayNumber);
            if (bay is null)
            {
                return this.NotFound();
            }

            return this.Ok(bay);
        }

        [HttpGet("{bayNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> GetByNumber(int bayNumber)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);
            if (bay is null)
            {
                return this.NotFound();
            }

            return this.Ok(bay);
        }

        #endregion
    }
}
