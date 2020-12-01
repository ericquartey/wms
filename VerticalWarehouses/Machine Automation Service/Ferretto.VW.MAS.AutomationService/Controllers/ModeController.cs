using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModeController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IMachineModeProvider machineModeProvider;

        #endregion

        #region Constructors

        public ModeController(IMachineModeProvider machineModeProvider)
        {
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<CommonUtils.Messages.MachineMode> Get()
        {
            return this.machineModeProvider.GetCurrent();
        }

        [HttpPost("automatic")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetAutomatic()
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Automatic);

            return this.Accepted();
        }

        [HttpPost("LoadUnitOperations")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetLoadUnitOperations()
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.LoadUnitOperations);

            return this.Accepted();
        }

        [HttpPost("manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetManual()
        {
            switch (this.BayNumber)
            {
                case BayNumber.BayOne:
                    this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual);
                    break;

                case BayNumber.BayTwo:
                    this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual2);
                    break;

                case BayNumber.BayThree:
                    this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual3);
                    break;

                default:
                    this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual);
                    break;
            }

            return this.Accepted();
        }

        #endregion
    }
}
