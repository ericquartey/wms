﻿using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements
{
    internal class RepetitiveHorizontalMovementsStartState : StateBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IRepetitiveHorizontalMovementsMachineData machineData;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServiceScope scope;

        private readonly ISensorsProvider sensorsProvider;

        private readonly IRepetitiveHorizontalMovementsStateData stateData;

        #endregion

        #region Constructors

        public RepetitiveHorizontalMovementsStartState(IRepetitiveHorizontalMovementsStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IRepetitiveHorizontalMovementsMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.sensorsProvider = this.scope.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.machineVolatileDataProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            this.machineResourcesProvider = this.scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogDebug($"1:Process Notification Message {message.Type} Source {message.Source}");
            if (message.Type == MessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        {
                            this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsPositionBayQuoteState(this.stateData, this.Logger));
                            break;
                        }
                    case MessageStatus.OperationError:
                        {
                            this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsErrorState(this.stateData, this.Logger));
                            break;
                        }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void Start()
        {
            this.Logger.LogDebug($"1:Start {this.GetType().Name} RequestingBay: {this.machineData.RequestingBay} TargetBay: {this.machineData.TargetBay}");

            var bayPositionId = this.machineData.MessageData.BayPositionId;

            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);

            // Check if vertical movement to the bay position quote is allowed
            var policy = this.CanMoveToBayPosition(bayPositionId);
            if (!policy.IsAllowed)
            {
                this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsErrorState(this.stateData, this.Logger));
                return;
            }

            switch (this.machineData.RequestingBay)
            {
                case BayNumber.BayOne:
                    this.machineVolatileDataProvider.Mode = MachineMode.Test;
                    break;

                case BayNumber.BayTwo:
                    this.machineVolatileDataProvider.Mode = MachineMode.Test2;
                    break;

                case BayNumber.BayThree:
                    this.machineVolatileDataProvider.Mode = MachineMode.Test3;
                    break;

                default:
                    this.machineVolatileDataProvider.Mode = MachineMode.Test;
                    break;
            }

            this.Logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");

            this.MoveToVerticalPosition(
                MovementMode.Position,
                bayPosition.Height,
                manualMovement: false,
                computeElongation: true,
                this.machineData.RequestingBay,
                MessageActor.DeviceManager,
                bayPositionId,
                targetCellId: null,
                waitContinue: false);

            // Publish a notification message
            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"Repetitive Horizontal Movements Started",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.RepetitiveHorizontalMovements,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart); ;

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsEndState(this.stateData, this.Logger));
        }

        /// <summary>
        /// Check if elevator can be move to the bay position.
        /// </summary>
        /// <param name="bayPositionId">Bay position Id</param>
        /// <returns></returns>
        private ActionPolicy CanMoveToBayPosition(int bayPositionId)
        {
            const double policyVerticalTolerance = 0.01;

            // check #1: the elevator is already in front of the specified position
            var currentBayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            if (currentBayPosition?.Id == bayPositionId && Math.Abs((currentBayPosition?.Id ?? 0f) - this.elevatorDataProvider.VerticalPosition) < policyVerticalTolerance)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsAlreadyLocatedOppositeToTheSpecifiedBayPosition", CommonUtils.Culture.Actual),
                    ReasonType = ReasonType.ElevatorInPosition
                };
            }

            // check #2: the elevator must be empty with pawl in zero position
            //           or
            //           the elevator must be full with pawl in non-zero position
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            var isChainInZeroPosition = this.machineResourcesProvider.IsSensorZeroOnCradle;
            var isElevatorFull = this.machineResourcesProvider.IsDrawerCompletelyOnCradle && loadingUnit != null;
            var isElevatorEmpty = this.machineResourcesProvider.IsDrawerCompletelyOffCradle && loadingUnit is null;

            if (!(isElevatorFull && !isChainInZeroPosition) && !(isElevatorEmpty && isChainInZeroPosition))
            {
                if (!isElevatorEmpty)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual),
                    };
                }
                else if (!isElevatorFull)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual),
                    };
                }
            }

            return ActionPolicy.Allowed;
        }

        /// <summary>
        /// Move the elevator to the target position.
        /// </summary>
        /// <param name="movementMode">MovementMode.Positioning: the given movement mode</param>
        /// <param name="targetPosition">The target position</param>
        /// <param name="manualMovement">false: it defines if movement is manual or automatic (guided)</param>
        /// <param name="computeElongation">Compute the elongation for the belt</param>
        /// <param name="requestingBay">The requesting bay</param>
        /// <param name="sender">DeviceManager: the sending actor</param>
        /// <param name="targetBayPositionId">The target bay position Id</param>
        /// <param name="targetCellId">null: the target cell Id</param>
        /// <param name="waitContinue">false: movement is not paused</param>
        /// <param name="isPickupMission">false: no mission operation</param>
        private void MoveToVerticalPosition(
            MovementMode movementMode,
            double targetPosition,
            bool manualMovement,
            bool computeElongation,
            BayNumber requestingBay,
            MessageActor sender,
            int? targetBayPositionId,
            int? targetCellId,
            bool waitContinue,
            bool isPickupMission = false)
        {
            //var homingDone = (checkHomingDone ? this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay] : true);

            // Bay homing is already performed successfully
            var homingDone = true;

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];
            if (movementMode == MovementMode.PositionAndMeasureWeight && !isLoadingUnitOnBoard)
            {
                this.Logger.LogWarning($"Do not measure weight on empty elevator!");
                movementMode = MovementMode.Position;
            }
            if (computeElongation && !isLoadingUnitOnBoard)
            {
                this.Logger.LogWarning($"Do not compute elongation on empty elevator!");
                computeElongation = false;
            }

            var manualParameters = manualMovement ? this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical) :
                                                    this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical);

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical, isLoadingUnitOnBoard);

            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var feedRate = manualParameters.FeedRateAfterZero;
            var speed = new[] { movementParameters.Speed * feedRate };

            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                movementMode,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Forwards)
            {
                LoadingUnitId = this.elevatorDataProvider.GetLoadingUnitOnBoard()?.Id,
                FeedRate = feedRate,
                ComputeElongation = computeElongation,
                TargetBayPositionId = targetBayPositionId,
                TargetCellId = targetCellId,
                WaitContinue = waitContinue,
                IsPickupMission = isPickupMission
            };

            this.Logger.LogInformation(
                $"MoveToVerticalPosition: {movementMode}; " +
                $"manualMovement: {manualMovement}; " +
                $"targetPosition: {targetPosition}; " +
                $"homing: {homingDone}; " +
                $"feedRate: {feedRate}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00}; " +
                $"speed w/o feedRate: {movementParameters.Speed:0.00}; " +
                $"LU id: {messageData.LoadingUnitId.GetValueOrDefault()}");

            var message = new CommandMessage(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);

            this.ParentStateMachine.PublishCommandMessage(message);
        }

        #endregion
    }
}
