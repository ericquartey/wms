using System;
using System.IO;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class InstallationStatusController : ControllerBase
    {
        #region Fields

        private readonly ISetupStatus dataLayerSetupStatus;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public InstallationStatusController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerSetupStatus = services.GetService(typeof(ISetupStatus)) as ISetupStatus;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(bool[]))]
        [ProducesResponseType(404)]
        [HttpGet("GetStatus")]
        public ActionResult<bool[]> GetStatusAsync()
        {
            return this.GetStatus_Method();
        }

        private ActionResult<bool[]> GetStatus_Method()
        {
            var value = new bool[23];
            try
            {
                value[0] = this.dataLayerSetupStatus.VerticalHomingDone;
                value[1] = this.dataLayerSetupStatus.HorizontalHomingDone;
                value[2] = this.dataLayerSetupStatus.BeltBurnishingDone;
                value[3] = this.dataLayerSetupStatus.VerticalResolutionDone;
                value[4] = this.dataLayerSetupStatus.VerticalOffsetDone;
                value[5] = this.dataLayerSetupStatus.CellsControlDone;
                value[6] = this.dataLayerSetupStatus.PanelsControlDone;
                value[7] = this.dataLayerSetupStatus.Shape1Done;
                value[8] = this.dataLayerSetupStatus.Shape2Done;
                value[9] = this.dataLayerSetupStatus.Shape3Done;
                value[10] = this.dataLayerSetupStatus.WeightMeasurementDone;
                value[11] = this.dataLayerSetupStatus.Shutter1Done;
                value[12] = this.dataLayerSetupStatus.Shutter2Done;
                value[13] = this.dataLayerSetupStatus.Shutter3Done;
                value[14] = this.dataLayerSetupStatus.Bay1ControlDone;
                value[15] = this.dataLayerSetupStatus.Bay2ControlDone;
                value[16] = this.dataLayerSetupStatus.Bay3ControlDone;
                value[17] = this.dataLayerSetupStatus.FirstDrawerLoadDone;
                value[18] = this.dataLayerSetupStatus.DrawersLoadedDone;
                value[19] = this.dataLayerSetupStatus.Laser1Done;
                value[20] = this.dataLayerSetupStatus.Laser2Done;
                value[21] = this.dataLayerSetupStatus.Laser3Done;
                value[22] = this.dataLayerSetupStatus.MachineDone;
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
