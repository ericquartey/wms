using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.DTOs;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallationController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly ISetupStatus dataLayerSetupStatus;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public InstallationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.dataLayerSetupStatus = services.GetService(typeof(ISetupStatus)) as ISetupStatus;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("ExecuteBeltBurnishing")]
        public void ExecuteBeltBurnishing([FromBody]BeltBurnishingMessageDataDTO data)
        {
            var cycles = data.CyclesQuantity;
            //TEMP Publish the event for up&down movements
        }

        [HttpGet("ExecuteHoming")]
        public void ExecuteHoming()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.Both);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(homingData, "Execute Homing Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Homing));
        }

        // Metodo richiamato per lo spostamento dello Shutter
        [HttpPost]
        [Route("ExecuteMovement")]
        public void ExecuteMovement([FromBody]MovementMessageDataDTO data)
        {
            var messageData = new MovementMessageData(data);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, "Execute Movement Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Movement));
        }

        // Metodo richiamato per lo spostamento dello Shutter
        [HttpPost]
        [Route("ExecuteShutterPositioningMovement")]
        public async Task ExecuteShutterPositioningMovementAsync([FromBody]ShutterPositioningMovementMessageDataDTO data)
        {
            switch (data.BayNumber)
            {
                case 1:
                    data.ShutterType = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter1Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case 2:
                    data.ShutterType = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter2Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case 3:
                    data.ShutterType = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter3Type, (long)ConfigurationCategory.GeneralInfo);
                    break;
            }

            var messageData = new ShutterPositioningMessageData(data);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, "Execute Movement Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Movement));
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<decimal>> GetDecimalConfigurationParameterAsync(string category, string parameter)
        {
            Enum.TryParse(typeof(VerticalAxis), parameter, out var parameterId);
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);

            if (parameterId != null)
            {
                decimal value;

                try
                {
                    value = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(Convert.ToInt64(parameterId), Convert.ToInt64(categoryId));
                }
                catch (Exception)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok(value);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        [ProducesResponseType(200, Type = typeof(bool[]))]
        [ProducesResponseType(500)]
        [HttpGet("GetInstallationStatus")]
        public async Task<ActionResult<bool[]>> GetInstallationStatus()
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
            catch (Exception exc)
            {
                return this.NotFound("Setup configuration not found");
            }

            return this.Ok(value);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<int>> GetIntegerConfigurationParameterAsync(string category, string parameter)
        {
            Enum.TryParse(typeof(Shutter1Control), parameter, out var parameterId);
            Enum.TryParse(typeof(ConfigurationCategory), category , out var categoryId);

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync(Convert.ToInt64(parameterId), Convert.ToInt64(categoryId));
                }
                catch (Exception)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok(value);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        [HttpGet("StartShutterControl/{delay}/{numberCycles}")]
        public async Task StartShutterControlAsync(int delay, int numberCycles)
        {
            IShutterControlData shutterControlData = new ShutterControlData(delay, numberCycles);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(shutterControlData, "Shutter Started", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ShutterControl));
        }

        [HttpGet("StopCommand")]
        public void StopCommand()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Stop));
        }

        #endregion
    }
}
