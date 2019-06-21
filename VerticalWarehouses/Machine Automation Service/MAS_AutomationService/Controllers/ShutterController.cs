using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
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
        public async Task ExecutePositioningAsync([FromBody]ShutterPositioningMovementMessageDataDTO data)
        {
            await this.ExecutePositioning_MethodAsync(data);
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

        private async Task ExecutePositioning_MethodAsync(ShutterPositioningMovementMessageDataDTO data)
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

        #endregion
    }
}
