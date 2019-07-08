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
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class PositioningController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public PositioningController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("Execute")]
        public async Task ExecuteAsync([FromBody]MovementMessageDataDto data)
        {
            await this.ExecutePositioning_MethodAsync(data);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private async Task ExecutePositioning_MethodAsync(MovementMessageDataDto data)
        {
            decimal maxSpeed = 0;
            decimal maxAcceleration = 0;
            decimal maxDeceleration = 0;
            decimal feedRate = 0;
            decimal initialTargetPosition = 0;
            decimal resolution = 0;

            try
            {
                var machineDone = await this.dataLayerConfigurationValueManagement.GetBoolConfigurationValueAsync(
                    (long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);

                switch (data.Axis)
                {
                    // INFO Vertical LSM
                    case Axis.Vertical:
                        maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
                        maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
                        maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
                        feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)VerticalManualMovements.FeedRate, (long)ConfigurationCategory.VerticalManualMovements);

                        if (machineDone)
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)VerticalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);
                        }
                        else
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)VerticalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);
                        }

                        // INFO +1 for Up, -1 for Down
                        initialTargetPosition *= data.Displacement;
                        resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

                        break;

                    // INFO Horizontal LSM
                    case Axis.Horizontal:
                        maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)HorizontalAxis.MaxSpeed, (long)ConfigurationCategory.HorizontalAxis);
                        maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)HorizontalAxis.MaxAcceleration, (long)ConfigurationCategory.HorizontalAxis);
                        maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)HorizontalAxis.MaxDeceleration, (long)ConfigurationCategory.HorizontalAxis);
                        feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)HorizontalManualMovements.FeedRate, (long)ConfigurationCategory.HorizontalManualMovements);

                        if (machineDone)
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)HorizontalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);
                        }
                        else
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);
                        }

                        initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);

                        // INFO +1 for Forward, -1 for Back
                        initialTargetPosition *= data.Displacement;
                        resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);

                        break;
                }

                var speed = maxSpeed * feedRate;

                var messageData = new PositioningMessageData(
                    data.Axis,
                    data.MovementType,
                    initialTargetPosition,
                    speed,
                    maxAcceleration,
                    maxDeceleration,
                    0,
                    0,
                    0,
                    resolution);
                this.eventAggregator.GetEvent<CommandEvent>().Publish(
                    new CommandMessage(
                        messageData,
                        $"Execute {data.Axis} Positioning Command",
                        MessageActor.FiniteStateMachines,
                        MessageActor.WebApi,
                        MessageType.Positioning));
            }
            catch (Exception)
            {
                // TODO
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
