using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompactingController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IMachineModeProvider machineModeProvider;

        #endregion

        #region Constructors

        public CompactingController(IMachineModeProvider machineModeProvider)
        {
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("compacting")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Compacting(bool optimizeRotationClass)
        {
            switch (this.BayNumber)
            {
                case BayNumber.BayOne:
                default:
                    this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Compact, optimizeRotationClass: optimizeRotationClass);
                    break;

                case BayNumber.BayTwo:
                    this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Compact2, optimizeRotationClass: optimizeRotationClass);
                    break;

                case BayNumber.BayThree:
                    this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Compact3, optimizeRotationClass: optimizeRotationClass);
                    break;
            }

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
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
