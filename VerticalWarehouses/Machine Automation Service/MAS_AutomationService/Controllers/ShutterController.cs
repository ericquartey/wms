using System;
using System.IO;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class ShutterController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ShutterController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("ExecuteControlTest/{bayNumber}/{delay}/{numberCycles}")]
        public void ExecuteControlTest(int bayNumber, int delay, int numberCycles)
        {
            this.ExecuteControlTest_Method(bayNumber, delay, numberCycles);
        }

        [HttpPost("ExecutePositioning")]
        public void ExecutePositioning([FromBody]ShutterPositioningMovementMessageDataDto data)
        {
            this.ExecutePositioning_Method(data);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            return this.GetIntegerConfigurationParameter_Method(category, parameter);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private void ExecuteControlTest_Method(int bayNumber, int delay, int numberCycles)
        {
            // TEMP Retrieve the max speed rate from the database
            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)ShutterHeightControl.FeedRate,
                ConfigurationCategory.ShutterHeightControl);

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

        private void ExecutePositioning_Method(ShutterPositioningMovementMessageDataDto data)
        {
            switch (data.ShutterType)
            {
                case ShutterType.NoType:
                    this.dataLayerConfigurationValueManagement
                        .GetIntegerConfigurationValue(
                            (long)GeneralInfo.Shutter1Type,
                            ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter2Type:
                    this.dataLayerConfigurationValueManagement
                        .GetIntegerConfigurationValue(
                            (long)GeneralInfo.Shutter2Type,
                            ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter3Type:
                    this.dataLayerConfigurationValueManagement
                        .GetIntegerConfigurationValue(
                            (long)GeneralInfo.Shutter3Type,
                            ConfigurationCategory.GeneralInfo);
                    break;
            }

            var maxSpeed = this.dataLayerConfigurationValueManagement
                .GetDecimalConfigurationValue(
                (long)ShutterHeightControl.FeedRate,
                ConfigurationCategory.ShutterHeightControl);

            //TEMP Speed rate parameter need to be multiply by 100
            //TEMP var speedRate = (Convert.ToDouble(maxSpeed) * 0.1) * 100;
            var speedRate = 100m;
            ShutterPosition destination;
            if (data.ShutterPositionMovement == ShutterMovementDirection.Up)
            {
                destination = ShutterPosition.Opened;
            }
            else
            {
                destination = ShutterPosition.Closed;
            }

            var messageData = new ShutterPositioningMessageData(
                destination,
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

        private ActionResult<int> GetIntegerConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);
            Enum.TryParse(typeof(BeltBurnishing), parameter, out var parameterId);
            var category = (ConfigurationCategory)categoryId;

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = this.dataLayerConfigurationValueManagement
                        .GetIntegerConfigurationValue(
                        (long)parameterId,
                        category);
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
