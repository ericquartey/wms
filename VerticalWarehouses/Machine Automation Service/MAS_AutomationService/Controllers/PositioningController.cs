using System;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
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

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        private readonly ISetupStatusDataLayer setupStatus;

        private readonly IVerticalAxisDataLayer verticalAxis;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovements;

        #endregion

        #region Constructors

        public PositioningController(IEventAggregator eventAggregator, IServiceProvider services, ILogger<PositioningController> logger)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.verticalAxis = services.GetService(typeof(IVerticalAxisDataLayer)) as IVerticalAxisDataLayer;
            this.verticalManualMovements = services.GetService(typeof(IVerticalManualMovementsDataLayer)) as IVerticalManualMovementsDataLayer;
            this.horizontalAxis = services.GetService(typeof(IHorizontalAxisDataLayer)) as IHorizontalAxisDataLayer;
            this.horizontalManualMovements = services.GetService(typeof(IHorizontalManualMovementsDataLayer)) as IHorizontalManualMovementsDataLayer;
            this.setupStatus = services.GetService(typeof(ISetupStatusDataLayer)) as ISetupStatusDataLayer;
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
                var machineDone = this.setupStatus.MachineDone;

                switch (data.Axis)
                {
                    // INFO Vertical LSM
                    case Axis.Vertical:
                        maxSpeed = this.verticalAxis.MaxEmptySpeed;
                        maxAcceleration = this.verticalAxis.MaxEmptyAcceleration;
                        maxDeceleration = this.verticalAxis.MaxEmptyDeceleration;
                        feedRate = this.verticalManualMovements.FeedRateVM;

                        if (machineDone)
                        {
                            initialTargetPosition = this.verticalManualMovements.RecoveryTargetPositionVM;
                        }
                        else
                        {
                            initialTargetPosition = this.verticalManualMovements.InitialTargetPositionVM;
                        }

                        // INFO +1 for Up, -1 for Down
                        initialTargetPosition *= data.Displacement;

                        break;

                    // INFO Horizontal LSM
                    case Axis.Horizontal:
                        maxSpeed = this.horizontalAxis.MaxEmptySpeedHA;
                        maxAcceleration = this.horizontalAxis.MaxEmptyAccelerationHA;
                        maxDeceleration = this.horizontalAxis.MaxEmptyDecelerationHA;
                        feedRate = this.horizontalManualMovements.FeedRateHM;

                        if (machineDone)
                        {
                            initialTargetPosition = this.horizontalManualMovements.RecoveryTargetPositionHM;
                        }
                        else
                        {
                            initialTargetPosition = this.horizontalManualMovements.InitialTargetPositionHM;
                        }

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
