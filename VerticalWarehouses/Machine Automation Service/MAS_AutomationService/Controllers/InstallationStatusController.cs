using System;
using System.IO;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class InstallationStatusController : ControllerBase
    {
        #region Methods

        [ProducesResponseType(200, Type = typeof(bool[]))]
        [ProducesResponseType(404)]
        [HttpGet]
        public ActionResult<bool[]> GetStatusAsync([FromServices] ISetupStatusDataLayer dataLayerSetupStatus)
        {
            var value = new bool[23];
            try
            {
                value[0] = dataLayerSetupStatus.VerticalHomingDone;
                value[1] = dataLayerSetupStatus.HorizontalHomingDone;
                value[2] = dataLayerSetupStatus.BeltBurnishingDone;
                value[3] = dataLayerSetupStatus.VerticalResolutionDone;
                value[4] = dataLayerSetupStatus.VerticalOffsetDone;
                value[5] = dataLayerSetupStatus.CellsControlDone;
                value[6] = dataLayerSetupStatus.PanelsControlDone;
                value[7] = dataLayerSetupStatus.Shape1Done;
                value[8] = dataLayerSetupStatus.Shape2Done;
                value[9] = dataLayerSetupStatus.Shape3Done;
                value[10] = dataLayerSetupStatus.WeightMeasurementDone;
                value[11] = dataLayerSetupStatus.Shutter1Done;
                value[12] = dataLayerSetupStatus.Shutter2Done;
                value[13] = dataLayerSetupStatus.Shutter3Done;
                value[14] = dataLayerSetupStatus.Bay1ControlDone;
                value[15] = dataLayerSetupStatus.Bay2ControlDone;
                value[16] = dataLayerSetupStatus.Bay3ControlDone;
                value[17] = dataLayerSetupStatus.FirstDrawerLoadDone;
                value[18] = dataLayerSetupStatus.DrawersLoadedDone;
                value[19] = dataLayerSetupStatus.Laser1Done;
                value[20] = dataLayerSetupStatus.Laser2Done;
                value[21] = dataLayerSetupStatus.Laser3Done;
                value[22] = dataLayerSetupStatus.MachineDone;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
            {
                return this.NotFound("Setup configuration not found");
            }

            return this.Ok(value);
        }

        #endregion
    }
}
