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
        #region Fields

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public ServicingController(
            IServicingProvider servicingProvider)
        {
            this.servicingProvider = servicingProvider ?? throw new System.ArgumentNullException(nameof(servicingProvider));
        }

        #endregion

        #region Methods

        [HttpPost("confirm-instruction")]
        public IActionResult ConfirmInstruction(int instructionId)
        {
            this.servicingProvider.ConfirmInstruction(instructionId);
            return this.Ok();
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

        [HttpGet("last-valid-servicing-info")]
        public ActionResult<ServicingInfo> GetLastValid()
        {
            return this.Ok(this.servicingProvider.GetLastValid());
        }

        [HttpGet("any-instruction-expired")]
        public ActionResult<bool> IsInstructionExpired()
        {
            return this.Ok(this.servicingProvider.IsAnyInstructionExpired());
        }

        [HttpGet("any-instruction-expiring")]
        public ActionResult<bool> IsInstructionExpiring()
        {
            return this.Ok(this.servicingProvider.IsAnyInstructionExpiring());
        }

        [HttpPost("refresh-description")]
        public IActionResult RefreshDescription(int servicingInfoId)
        {
            this.servicingProvider.RefreshDescription(servicingInfoId);
            return this.Ok();
        }

        [HttpPost("set-note")]
        public IActionResult SetNote(string maintainerName, string note, int ID)
        {
            this.servicingProvider.SetNote(maintainerName, note, ID);
            return this.Ok();
        }

        [HttpGet("get-statistic")]
        public ActionResult<MachineStatistics> GetStatistic(int ID)
        {
            return this.Ok(this.servicingProvider.GetSettings(ID));
        }

        [HttpPost("update-service-status")]
        public IActionResult UpdateServiceStatus()
        {
            this.servicingProvider.UpdateServiceStatus();
            return this.Ok();
        }

        #endregion
    }
}
