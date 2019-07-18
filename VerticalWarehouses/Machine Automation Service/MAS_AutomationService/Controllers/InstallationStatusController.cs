using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
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
        public async Task<ActionResult<bool[]>> GetStatusAsync()
        {
            return await this.GetStatus_MethodAsync();
        }

        private async Task<ActionResult<bool[]>> GetStatus_MethodAsync()
        {
            var value = new bool[23];
            try
            {
                value[0] = await this.dataLayerSetupStatus.VerticalHomingDone;
                value[1] = await this.dataLayerSetupStatus.HorizontalHomingDone;
                value[2] = await this.dataLayerSetupStatus.BeltBurnishingDone;
                value[3] = await this.dataLayerSetupStatus.VerticalResolutionDone;
                value[4] = await this.dataLayerSetupStatus.VerticalOffsetDone;
                value[5] = await this.dataLayerSetupStatus.CellsControlDone;
                value[6] = await this.dataLayerSetupStatus.PanelsControlDone;
                value[7] = await this.dataLayerSetupStatus.Shape1Done;
                value[8] = await this.dataLayerSetupStatus.Shape2Done;
                value[9] = await this.dataLayerSetupStatus.Shape3Done;
                value[10] = await this.dataLayerSetupStatus.WeightMeasurementDone;
                value[11] = await this.dataLayerSetupStatus.Shutter1Done;
                value[12] = await this.dataLayerSetupStatus.Shutter2Done;
                value[13] = await this.dataLayerSetupStatus.Shutter3Done;
                value[14] = await this.dataLayerSetupStatus.Bay1ControlDone;
                value[15] = await this.dataLayerSetupStatus.Bay2ControlDone;
                value[16] = await this.dataLayerSetupStatus.Bay3ControlDone;
                value[17] = await this.dataLayerSetupStatus.FirstDrawerLoadDone;
                value[18] = await this.dataLayerSetupStatus.DrawersLoadedDone;
                value[19] = await this.dataLayerSetupStatus.Laser1Done;
                value[20] = await this.dataLayerSetupStatus.Laser2Done;
                value[21] = await this.dataLayerSetupStatus.Laser3Done;
                value[22] = await this.dataLayerSetupStatus.MachineDone;
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
