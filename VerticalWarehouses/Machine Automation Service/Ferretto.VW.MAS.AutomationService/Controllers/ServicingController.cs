using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicingController : ControllerBase
    {
        private readonly IServicingProvider servicingProvider;

        public ServicingController(
            IServicingProvider servicingProvider)
        {
            this.servicingProvider = servicingProvider ?? throw new System.ArgumentNullException(nameof(servicingProvider));
        }

        [HttpPost("confirm-service")]
        public IActionResult ConfirmService()
        {
            this.servicingProvider.ConfirmService();
            return this.Ok();
        }

        [HttpPost("confirm-setup")]
        public IActionResult ConfirmSetup()
        {
            this.servicingProvider.ConfirmSetup();
            return this.Ok();
        }

        [HttpGet("actual-servicing-info")]
        public ActionResult<ServicingInfo> GetActual()
        {
            return this.Ok(this.servicingProvider.GetActual());
        }

        [HttpGet("all-servicing-info")]
        public ActionResult<IEnumerable<ServicingInfo>> GetAll()
        {
            return this.Ok(this.servicingProvider.GetAll());
        }

        [HttpGet("getById-servicing-info")]
        public ActionResult<ServicingInfo> GetById(int id)
        {
            return this.Ok(this.servicingProvider.GetById(id));
        }

        [HttpGet("installation-info")]
        public ActionResult<ServicingInfo> GetInstallationInfo()
        {
            return this.Ok(this.servicingProvider.GetInstallationInfo());
        }

        [HttpGet("last-servicing-info")]
        public ActionResult<ServicingInfo> GetLastConfirmed()
        {
            return this.Ok(this.servicingProvider.GetLastConfirmed());
        }
    }
}
