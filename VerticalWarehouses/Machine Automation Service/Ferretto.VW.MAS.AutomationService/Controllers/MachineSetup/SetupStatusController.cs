using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/setup/[controller]")]
    [ApiController]
    public class SetupStatusController : ControllerBase
    {
        #region Fields

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public SetupStatusController(ISetupStatusProvider setupStatusProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.setupStatusProvider = setupStatusProvider ?? throw new System.ArgumentNullException(nameof(setupStatusProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new System.ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        [HttpPost("bay-carousel-calibration-bypass")]
        public IActionResult BayCarouselCalibrationBypass(BayNumber bayNumber)
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayCarouselCalibration(bayNumber), true));
        }

        [HttpPost("bay-first-loading-unit-bypass")]
        public IActionResult BayFirstLoadingUnitBypass(BayNumber bayNumber)
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayFirstLoadingUnit(bayNumber), true));
        }

        [HttpPost("bay-height-check-bypass")]
        public IActionResult BayHeightCheckBypass(BayNumber bayNumber)
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayHeightCheck(bayNumber), true));
        }

        [HttpPost("bay-laser-bypass")]
        public IActionResult BayLaserBypass(BayNumber bayNumber)
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayLaser(bayNumber), true));
        }

        [HttpPost("bay-profile-check-bypass")]
        public IActionResult BayProfileCheckBypass(BayNumber bayNumber)
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayProfileCheck(bayNumber), true));
        }

        [HttpPost("bay-shutter-test-bypass")]
        public IActionResult BayShutterTestBypass(BayNumber bayNumber)
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayShutterTest(bayNumber), true));
        }

        [HttpPost("belt-burnishing-test-bypass")]
        public IActionResult BeltBurnishingTestBypass()
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBeltBurnishingTest(), true));
        }

        [HttpPost("cells-height-check-bypass")]
        public IActionResult CellsHeightCheckBypass()
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetCellsHeightCheck(), true));
        }

        [HttpPost("cell-panels-check-bypass")]
        public IActionResult CellsPanelCheckBypass()
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetCellPanelsCheck(), true));
        }

        [HttpPost("deposit-and-pickup-test-bypass")]
        public IActionResult DepositAndPickUpTestBypass()
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetDepositAndPickUpTest(), true));
        }

        [HttpGet]
        public ActionResult<SetupStatusCapabilities> Get()
        {
            return this.Ok(this.setupStatusProvider.Get());
        }

        [HttpPost("load-first-drawer-test-bypass")]
        public IActionResult LoadFirstDrawerTestBypass()
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetLoadFirstDrawerTest(), true));
        }

        [HttpPost("shutter-height-check-bypass")]
        public IActionResult ShutterHeightCheckBypass()
        {
            return this.Ok(this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetShutterHeightCheck(), true));
        }

        #endregion
    }
}
