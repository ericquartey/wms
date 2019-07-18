using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class ShutterController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ShutterController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("ExecuteControlTest/{bayNumber}/{delay}/{numberCycles}")]
        public async Task ExecuteControlTestAsync(int bayNumber, int delay, int numberCycles)
        {
            await this.ExecuteControlTest_MethodAsync(bayNumber, delay, numberCycles);
        }

        [HttpPost("ExecutePositioning")]
        public async Task ExecutePositioningAsync([FromBody]ShutterPositioningMovementMessageDataDto data)
        {
            await this.ExecutePositioning_MethodAsync(data);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<int>> GetIntegerConfigurationParameterAsync(string category, string parameter)
        {
            return await this.GetIntegerConfigurationParameter_MethodAsync(category, parameter);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private async Task ExecuteControlTest_MethodAsync(int bayNumber, int delay, int numberCycles)
        {
            // TEMP Retrieve the max speed rate from the database
            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)ShutterHeightControl.FeedRate, (long)ConfigurationCategory.ShutterHeightControl);

            var shutterControlMessageData = new ShutterControlMessageData(
                bayNumber,
                delay,
                numberCycles,
                (int)maxSpeed);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    shutterControlMessageData,
                    "Shutter Started",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.ShutterControl));
        }

        private async Task ExecutePositioning_MethodAsync(ShutterPositioningMovementMessageDataDto data)
        {
            switch (data.ShutterType)
            {
                case ShutterType.NoType:
                    await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync(
                        (long)GeneralInfo.Shutter1Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter2Type:
                    await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync(
                        (long)GeneralInfo.Shutter2Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter3Type:
                    await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync(
                        (long)GeneralInfo.Shutter3Type, (long)ConfigurationCategory.GeneralInfo);
                    break;
            }

            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)ShutterHeightControl.FeedRate, (long)ConfigurationCategory.ShutterHeightControl);

            //TEMP Speed rate parameter need to be multiply by 100
            //TEMP var speedRate = (Convert.ToDouble(maxSpeed) * 0.1) * 100;
            var speedRate = 100m;

            var messageData = new ShutterPositioningMessageData(
                ShutterPosition.Closed,
                data.ShutterPositionMovement,
                ShutterType.Shutter3Type,
                data.BayNumber,
                speedRate);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    messageData,
                    "Execute Shutter Positioning Movement Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.ShutterPositioning));
        }

        private async Task<ActionResult<int>> GetIntegerConfigurationParameter_MethodAsync(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(BeltBurnishing), parameter, out var parameterId);

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)parameterId, (long)categoryId);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
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

        private void Stop_Method()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    null,
                    "Stop Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Stop));
        }

        #endregion
    }
}
