using System;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Microsoft.AspNetCore.Http;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShuttersController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        private readonly IShutterTestParametersProvider shutterTestParametersProvider;

        #endregion

        #region Constructors

        public ShuttersController(
            IEventAggregator eventAggregator,
            IShutterTestParametersProvider shutterTestParametersProvider,
            IConfigurationValueManagmentDataLayer configurationProvider)
            : base(eventAggregator)
        {
            if (shutterTestParametersProvider == null)
            {
                throw new ArgumentNullException(nameof(shutterTestParametersProvider));
            }

            if (configurationProvider == null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            this.shutterTestParametersProvider = shutterTestParametersProvider;
            this.configurationProvider = configurationProvider;
        }

        #endregion

        #region Methods

        [HttpGet("shutters/position/{bayNumber}")]
        public ActionResult<ShutterPosition> GetShutterPosition(int bayNumber)
        {
            var messageData = new RequestPositionMessageData(Axis.None, bayNumber);
            this.PublishCommand(
                messageData,
                "Request vertical position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);

            //this.logger.LogDebug($"Request position on BayNumber {bayNumber}");

            var notifyData = this.WaitForResponseEventAsync<ShutterPositioningMessageData>(
                MessageType.ShutterPositioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting);

            return this.Ok(notifyData?.ShutterPosition ?? ShutterPosition.None);
        }

        [HttpGet]
        public ActionResult<ShutterTestParameters> GetTestParameters()
        {
            var parameters = this.shutterTestParametersProvider.Get();

            return this.Ok(parameters);
        }

        [HttpPost("{bayNumber}/move")]
        public void Move(int bayNumber, [FromBody]ShutterPositioningMovementMessageDataDto data)
        {
            switch (data.ShutterType)
            {
                case ShutterType.NoType:
                    this.configurationProvider
                        .GetIntegerConfigurationValue(
                            (long)GeneralInfo.Shutter1Type,
                            ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter2Type:
                    this.configurationProvider
                        .GetIntegerConfigurationValue(
                            (long)GeneralInfo.Shutter2Type,
                            ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter3Type:
                    this.configurationProvider
                        .GetIntegerConfigurationValue(
                            (long)GeneralInfo.Shutter3Type,
                            ConfigurationCategory.GeneralInfo);
                    break;
            }

            var maxSpeed = this.configurationProvider
                .GetDecimalConfigurationValue(
                    (long)ShutterHeightControl.FeedRate,
                    ConfigurationCategory.ShutterHeightControl);

            // TODO what is this?

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

            this.PublishCommand(
                messageData,
                "Execute Shutter Positioning Movement Command",
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning);
        }

        [HttpPost("{bayNumber}/run-test")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult RunTest(int bayNumber, int delay, int numberCycles)
        {
            if (delay <= 0)
            {
                return this.BadRequest("Delay must be strictly positive.");
            }

            if (numberCycles <= 0)
            {
                return this.BadRequest("NumberCycles must be strictly positive.");
            }

            // TODO Retrieve the max speed rate from the database
            var maxSpeed = this.configurationProvider.GetDecimalConfigurationValue(
                (long)ShutterHeightControl.FeedRate,
                ConfigurationCategory.ShutterHeightControl);

            var shutterControlMessageData = new ShutterTestStatusChangedMessageData(
                bayNumber,
                delay,
                numberCycles,
                (int)maxSpeed);

            this.PublishCommand(
                shutterControlMessageData,
                "Shutter Started",
                MessageActor.FiniteStateMachines,
                MessageType.ShutterTestStatusChanged);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.PublishCommand(
                null,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
