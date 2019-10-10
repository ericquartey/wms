using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepositAndPickupProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationValueManagement;

        private readonly IDepositAndPickUpDataLayer depositAndPickUpDataLayer;

        private readonly IElevatorProvider elevatorProvider;

        #endregion

        #region Constructors

        public DepositAndPickupProcedureController(
                IEventAggregator eventAggregator,
                IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
                IDepositAndPickUpDataLayer depositAndPickUpDataLayer,
                IElevatorProvider elevatorProvider)
                : base(eventAggregator)
        {
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.configurationValueManagement = dataLayerConfigurationValueManagement ?? throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            this.depositAndPickUpDataLayer = depositAndPickUpDataLayer ?? throw new ArgumentNullException(nameof(depositAndPickUpDataLayer));
        }

        #endregion

        #region Methods

        [HttpGet("cycle-quantity")]
        public ActionResult<int> GetCycleQuantity()
        {
            return this.Ok(this.elevatorProvider.GetDepositAndPickUpCycleQuantity());
        }

        [HttpGet("required-cycle-quantity")]
        public ActionResult<int> GetRequiredCycleQuantity()
        {
            var requiredCycles = this.depositAndPickUpDataLayer.CycleQuantityDP;
            return this.Ok(requiredCycles);
        }

        [HttpPost("increase-cycle-quantity")]
        public IActionResult IncreaseCycleQuantity()
        {
            this.elevatorProvider.IncreaseDepositAndPickUpCycleQuantity();
            return this.Ok();
        }

        [HttpPost("reset-cycle-quantity")]
        public IActionResult Reset()
        {
            this.elevatorProvider.ResetDepositAndPickUpCycleQuantity();
            return this.Ok();
        }

        #endregion
    }
}
