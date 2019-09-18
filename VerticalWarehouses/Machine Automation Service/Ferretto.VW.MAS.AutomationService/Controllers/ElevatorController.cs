using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : BaseAutomationController
    {
        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        private readonly IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider;

        private readonly IMachineConfigurationProvider machineConfigurationProvider;

        #endregion

        #region Constructors

        public ElevatorController(
            IEventAggregator eventAggregator,
            IElevatorProvider elevatorProvider,
            IMachineConfigurationProvider machineConfigurationProvider,
            IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider)
            : base(eventAggregator)
        {
            if (elevatorProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorProvider));
            }

            if (elevatorWeightCheckProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorWeightCheckProvider));
            }
            if (machineConfigurationProvider is null)
            {
                throw new ArgumentNullException(nameof(machineConfigurationProvider));
            }

            this.elevatorProvider = elevatorProvider;
            this.elevatorWeightCheckProvider = elevatorWeightCheckProvider;
            this.machineConfigurationProvider = machineConfigurationProvider;
        }

        #endregion

        #region Methods

        [HttpGet("horizontal/position")]
        public ActionResult<decimal> GetHorizontalPosition()
        {
            try
            {
                var position = this.elevatorProvider.GetHorizontalPosition(this.BayNumber);
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<decimal>(ex);
            }
        }

        [HttpGet("vertical/position")]
        public ActionResult<decimal> GetVerticalPosition()
        {
            try
            {
                var position = this.elevatorProvider.GetVerticalPosition(this.BayNumber);
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<decimal>(ex);
            }
        }

        [HttpPost("horizontal/move-auto")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard)
        {
            try
            {
                void publishAction()
                {
                    this.PublishCommand(
                        null,
                        "Sensors changed Command",
                        MessageActor.FiniteStateMachines,
                        MessageType.SensorsChanged);
                }

                var messageData = this.WaitForResponseEventAsync<SensorsChangedMessageData>(
                    MessageType.SensorsChanged,
                    MessageActor.FiniteStateMachines,
                    MessageStatus.OperationExecuting,
                    publishAction);

                // check feasibility
                if (isStartedOnBoard != (messageData.SensorsStates[(int)IOMachineSensors.LuPresentInMachineSideBay1] && messageData.SensorsStates[(int)IOMachineSensors.LuPresentInOperatorSideBay1]))
                {
                    throw new InvalidOperationException("Invalid " + (isStartedOnBoard ? "Deposit" : "Pickup") + " command for " + (isStartedOnBoard ? "empty" : "full") + " elevator");
                }
                var zeroSensor = (this.machineConfigurationProvider.IsOneKMachine() ? IOMachineSensors.ZeroPawlSensorOneK : IOMachineSensors.ZeroPawlSensor);
                if ((!isStartedOnBoard && !messageData.SensorsStates[(int)zeroSensor])
                    || (isStartedOnBoard && messageData.SensorsStates[(int)zeroSensor])
                    )
                {
                    throw new InvalidOperationException("Invalid Zero Chain position");
                }

                // execute command
                var position = this.elevatorProvider.GetHorizontalPosition();
                this.elevatorProvider.MoveHorizontalAuto(direction, isStartedOnBoard, position.Value);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("horizontal/move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalManual(HorizontalMovementDirection direction)
        {
            try
            {
                this.elevatorProvider.MoveHorizontal(direction, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/move-to")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory)
        {
            try
            {
                this.elevatorProvider.MoveToVerticalPosition(targetPosition, feedRateCategory, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVertical(VerticalMovementDirection direction)
        {
            try
            {
                this.elevatorProvider.MoveVertical(direction, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/move-relative")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVerticalOfDistance(decimal distance)
        {
            try
            {
                this.elevatorProvider.MoveVerticalOfDistance(distance, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            try
            {
                this.elevatorProvider.Stop(this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("weight-check-stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StopWeightCheck()
        {
            try
            {
                this.elevatorWeightCheckProvider.Stop();
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/resolution")]
        public IActionResult UpdateResolution(decimal newResolution)
        {
            try
            {
                this.elevatorProvider.UpdateResolution(newResolution);

                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("weight-check")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult WeightCheck(int loadingUnitId, decimal runToTest, decimal weight)
        {
            try
            {
                this.elevatorWeightCheckProvider.Start(loadingUnitId, runToTest, weight);

                var data = new ElevatorWeightCheckMessageData() { Weight = 200 };

                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        #endregion
    }
}
