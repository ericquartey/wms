using System;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
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
            if (baysProvider == null)
            {
                throw new ArgumentNullException(nameof(baysProvider));
            }

            this.baysProvider = baysProvider;
        }

        #endregion

        #region Methods

        [HttpPost("{id}/activate")]
        public ActionResult<Bay> ActivateAsync(int id)
        {
            var bay = this.baysProvider.Activate(id);
            if (bay == null)
            {
                return this.NotFound();
            }

            return this.Ok();
        }

        [HttpPost("{id}/deactivate")]
        public ActionResult<Bay> DeactivateAsync(int id)
        {
            var bay = this.baysProvider.Deactivate(id);
            if (bay == null)
            {
                return this.NotFound();
            }

            return this.Ok();
        }

        #endregion
    }
}
