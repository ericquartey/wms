using System;
using System.Threading.Tasks;
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
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class OffsetCalibrationController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public OffsetCalibrationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("ExecuteCompleted")]
        public async Task<bool> ExecuteCompletedAsync()
        {
            return await this.ExecuteCompleted_MethodAsync();
        }

        [HttpGet("ExecutePositioning")]
        public async Task ExecutePositioningAsync()
        {
            await this.ExecutePositioning_MethodAsync();
        }

        [HttpGet("ExecuteStepDown")]
        public async Task ExecuteStepDownAsync()
        {
            var stepValue = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

            await this.ExecuteStep_MethodAsync(-stepValue);
        }

        [HttpGet("ExecuteStepUp")]
        public async Task ExecuteStepUpAsync()
        {
            var stepValue = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

            await this.ExecuteStep_MethodAsync(stepValue);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [HttpPost("SetOffsetParameter/{newOffset}/")]
        public async Task<bool> SetOffsetParameterAsync(decimal newOffset)
        {
            return await this.SetOffsetParameter_MethodAsync(newOffset);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private async Task<bool> ExecuteCompleted_MethodAsync()
        {
            var completionPersist = true;

            try
            {
                await this.dataLayerConfigurationValueManagement.SetBoolConfigurationValueAsync((long)SetupStatus.VerticalOffsetDone, (long)ConfigurationCategory.SetupStatus, true);
            }
            catch (Exception ex)
            {
                completionPersist = false;
            }

            return completionPersist;
        }

        private async Task ExecutePositioning_MethodAsync()
        {
            var referenceCell = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync(
                (long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

            var position = 10; //TODO Retrieve the position related to the cellReference value

            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                position,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0,
                resolution);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private async Task ExecuteStep_MethodAsync(decimal displacement)
        {
            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                displacement,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0,
                resolution);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private async Task<bool> SetOffsetParameter_MethodAsync(decimal newOffset)
        {
            var resultAssignment = true;

            try
            {
                await this.dataLayerConfigurationValueManagement.SetDecimalConfigurationValueAsync((long)VerticalAxis.Offset, (long)ConfigurationCategory.VerticalAxis, newOffset);
            }
            catch (Exception ex)
            {
                resultAssignment = false;
            }

            return resultAssignment;
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
