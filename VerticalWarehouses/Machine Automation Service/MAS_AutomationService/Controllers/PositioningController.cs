using System;
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
    public class PositioningController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public PositioningController(IEventAggregator eventAggregator, IServiceProvider services, ILogger<PositioningController> logger)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.logger = logger;
        }

        #endregion

        #region Methods

        [HttpPost("Execute")]
        public void ExecuteAsync([FromBody]MovementMessageDataDto data)
        {
            this.ExecutePositioning_Method(data);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private void ExecutePositioning_Method(MovementMessageDataDto data)
        {
            decimal maxSpeed = 0;
            decimal maxAcceleration = 0;
            decimal maxDeceleration = 0;
            decimal feedRate = 0;
            decimal initialTargetPosition = 0;

            try
            {
                var machineDone = this.dataLayerConfigurationValueManagement.GetBoolConfigurationValue(
                    (long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);

                switch (data.Axis)
                {
                    // INFO Vertical LSM
                    case Axis.Vertical:
                        maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)VerticalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.VerticalAxis);
                        maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)VerticalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.VerticalAxis);
                        feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)VerticalManualMovements.FeedRate, (long)ConfigurationCategory.VerticalManualMovements);

                        if (machineDone)
                        {
                            initialTargetPosition = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                                (long)VerticalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);
                        }
                        else
                        {
                            initialTargetPosition = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                                (long)VerticalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);
                        }

                        // INFO +1 for Up, -1 for Down
                        initialTargetPosition *= data.Displacement;

                        break;

                    // INFO Horizontal LSM
                    case Axis.Horizontal:
                        maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)HorizontalAxis.MaxEmptySpeed, (long)ConfigurationCategory.HorizontalAxis);
                        maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)HorizontalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.HorizontalAxis);
                        maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)HorizontalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.HorizontalAxis);
                        feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)HorizontalManualMovements.FeedRate, (long)ConfigurationCategory.HorizontalManualMovements);

                        if (machineDone)
                        {
                            initialTargetPosition = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                                (long)HorizontalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);
                        }
                        else
                        {
                            initialTargetPosition = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                                (long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);
                        }

                        initialTargetPosition = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                            (long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

                        // INFO +1 for Forward, -1 for Back
                        initialTargetPosition *= data.Displacement;

                        break;
                }

                var speed = maxSpeed * feedRate;

                var messageData = new PositioningMessageData(
                    data.Axis,
                    data.MovementType,
                    MovementMode.Position,
                    initialTargetPosition,
                    speed,
                    maxAcceleration,
                    maxDeceleration,
                    0,
                    0,
                    0);
                this.eventAggregator.GetEvent<CommandEvent>().Publish(
                    new CommandMessage(
                        messageData,
                        $"Execute {data.Axis} Positioning Command",
                        MessageActor.FiniteStateMachines,
                        MessageActor.WebApi,
                        MessageType.Positioning));

                this.logger.LogDebug($"Starting positioning on Axis {data.Axis}, type {data.MovementType}, target position {initialTargetPosition}");
            }
            catch (Exception ex)
            {
                throw ex;
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
