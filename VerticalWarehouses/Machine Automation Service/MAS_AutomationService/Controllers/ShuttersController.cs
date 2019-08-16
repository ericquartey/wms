using System;
using System.IO;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Microsoft.AspNetCore.Http;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShuttersController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        #endregion

        #region Constructors

        public ShuttersController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement)
            : base(eventAggregator)
        {
            if (dataLayerConfigurationValueManagement == null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            }

            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
        }

        #endregion

        #region Methods

        [HttpGet("integer-configuration-parameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(BeltBurnishing), parameter, out var parameterId);
            var categoryEnum = (ConfigurationCategory)categoryId;

            if (parameterId != null)
            {
                try
                {
                    return this.dataLayerConfigurationValueManagement
                        .GetIntegerConfigurationValue(
                        (long)parameterId,
                        categoryEnum);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        [HttpPost("{bayNumber}/move")]
        public void Move(int bayNumber, [FromBody]ShutterPositioningMovementMessageDataDto data)
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

            var messageData = new ShutterPositioningMessageData(
                ShutterPosition.Closed,
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

            // TEMP Retrieve the max speed rate from the database
            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
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
                MessageType.ShutterControl);

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
