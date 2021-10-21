using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/setup/[controller]")]
    [ApiController]
    public class SetupStatusController : ControllerBase, IRequestingBayController
    {
        //private readonly IServicingProvider servicingProvider;

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

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("bay-carousel-calibration-bypass")]
        public IActionResult BayCarouselCalibrationBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayCarouselCalibration(this.BayNumber), true);
            return this.Ok();
        }

        [HttpPost("bay-external-calibration-bypass")]
        public IActionResult BayExternalCalibrationBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayExternalCalibration(this.BayNumber), true);
            return this.Ok();
        }

        [HttpPost("bay-height-check-bypass")]
        public IActionResult BayHeightCheckBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayHeightCheck(this.BayNumber), true);
            return this.Ok();
        }

        [HttpPost("bay-laser-bypass")]
        public IActionResult BayLaserBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayLaser(this.BayNumber), true);
            return this.Ok();
        }

        [HttpPost("bay-profile-check-bypass")]
        public IActionResult BayProfileCheckBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayProfileCheck(this.BayNumber), true);
            return this.Ok();
        }

        [HttpPost("bay-shutter-test-bypass")]
        public IActionResult BayShutterTestBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayShutterTest(this.BayNumber), true);
            return this.Ok();
        }

        [HttpPost("belt-burnishing-test-bypass")]
        public IActionResult BeltBurnishingTestBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBeltBurnishingTest(), true);
            return this.Ok();
        }

        [HttpPost("cells-height-check-bypass")]
        public IActionResult CellsHeightCheckBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetCellsHeightCheck(), true);
            return this.Ok();
        }

        [HttpPost("cell-panels-check-bypass")]
        public IActionResult CellsPanelCheckBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetCellPanelsCheck(), true);
            return this.Ok();
        }

        [HttpPost("deposit-and-pickup-test-bypass")]
        public IActionResult DepositAndPickUpTestBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetDepositAndPickUpTest(), true);
            return this.Ok();
        }

        [HttpPost("full-test-bypass")]
        public IActionResult FullTestBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetFullTest(this.BayNumber), true);
            return this.Ok();
        }

        [HttpGet]
        public ActionResult<SetupStatusCapabilities> Get()
        {
            return this.Ok(this.setupStatusProvider.Get());
        }

        [HttpPost("horizontal-chain-calibration-bypass")]
        public IActionResult HorizontalChainCalibrationBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetHorizontalChainCalibration(), true);
            return this.Ok();
        }

        [HttpPost("horizontal-resolution-calibration-bypass")]
        public IActionResult HorizontalResolutionCalibrationBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetHorizontalResolutionCalibration(), true);
            return this.Ok();
        }

        [HttpPost("load-first-drawer-test-bypass")]
        public IActionResult LoadFirstDrawerTestBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetLoadFirstDrawerTest(), true);
            return this.Ok();
        }

        [HttpPost("shutter-height-check-bypass")]
        public IActionResult ShutterHeightCheckBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetShutterHeightCheck(), true);
            return this.Ok();
        }

        [HttpPost("weight-measurement-bypass")]
        public IActionResult WeightMeasurementBypass()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetWeightMeasurement(), true);
            return this.Ok();
        }

        #endregion
    }
}
