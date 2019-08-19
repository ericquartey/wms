using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallationStatusController : ControllerBase
    {
        #region Methods

        [HttpGet]
        public ActionResult<bool[]> Get([FromServices] ISetupStatusDataLayer dataLayerSetupStatus)
        {
            var value = new bool[]
            {
                dataLayerSetupStatus.VerticalHomingDone,
                dataLayerSetupStatus.HorizontalHomingDone,
                dataLayerSetupStatus.BeltBurnishingDone,
                dataLayerSetupStatus.VerticalResolutionDone,
                dataLayerSetupStatus.VerticalOffsetDone,
                dataLayerSetupStatus.CellsControlDone,
                dataLayerSetupStatus.PanelsControlDone,
                dataLayerSetupStatus.Shape1Done,
                dataLayerSetupStatus.Shape2Done,
                dataLayerSetupStatus.Shape3Done,
                dataLayerSetupStatus.WeightMeasurementDone,
                dataLayerSetupStatus.Shutter1Done,
                dataLayerSetupStatus.Shutter2Done,
                dataLayerSetupStatus.Shutter3Done,
                dataLayerSetupStatus.Bay1ControlDone,
                dataLayerSetupStatus.Bay2ControlDone,
                dataLayerSetupStatus.Bay3ControlDone,
                dataLayerSetupStatus.FirstDrawerLoadDone,
                dataLayerSetupStatus.DrawersLoadedDone,
                dataLayerSetupStatus.Laser1Done,
                dataLayerSetupStatus.Laser2Done,
                dataLayerSetupStatus.Laser3Done,
                dataLayerSetupStatus.MachineDone,
            };

            return this.Ok(value);
        }

        #endregion
    }
}
