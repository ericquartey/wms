using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal partial class ElevatorProvider
    {
        #region Methods

        /// <summary>
        /// Moves the horizontal chain of the elevator to load or unload a LoadUnit for the machine of 1 Ton.
        /// It uses a combination of indipendently horizontal movement and vertical movement.
        /// </summary>
        private void MoveHorizontalAuto_ForOneTonMachine(
            HorizontalMovementDirection direction,
            bool isLoadingUnitOnBoard,
            int? loadingUnitId,
            double? loadingUnitGrossWeight,
            bool waitContinue,
            bool measure,
            BayNumber requestingBay,
            MessageActor sender,
            int? targetCellId = null,
            int? targetBayPositionId = null,
            int? sourceCellId = null,
            int? sourceBayPositionId = null,
            bool fastDeposit = true)
        {
            if (loadingUnitId.HasValue
                &&
                loadingUnitGrossWeight.HasValue)
            {
                this.loadingUnitsDataProvider.SetWeight(loadingUnitId.Value, loadingUnitGrossWeight.Value);
            }

            var sensors = this.sensorsProvider.GetAll();

            var zeroSensor = this.machineVolatileDataProvider.IsOneTonMachine.Value
                ? IOMachineSensors.ZeroPawlSensorOneTon
                : IOMachineSensors.ZeroPawlSensor;

            if (!isLoadingUnitOnBoard && !sensors[(int)zeroSensor])
            {
                throw new InvalidOperationException(Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual));
            }
            if (isLoadingUnitOnBoard && sensors[(int)zeroSensor])
            {
                throw new InvalidOperationException(Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual));
            }

            if (measure && isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not measure profile on full elevator!");
                measure = false;
            }

            var profileType = this.SelectProfileType(direction, isLoadingUnitOnBoard);

            var horizontalAxis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var center = horizontalAxis.Center;

            var profileSteps = horizontalAxis.Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number)
                .ToList();

            if (!loadingUnitId.HasValue && isLoadingUnitOnBoard)
            {
                var loadUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadUnit != null)
                {
                    loadingUnitId = loadUnit.Id;
                }
            }

            // if weight is unknown we move as full weight
            double scalingFactor = 1;
            if (loadingUnitId.HasValue
                && !measure
                && this.machineVolatileDataProvider.Mode != MachineMode.FirstTest
                && this.machineVolatileDataProvider.Mode != MachineMode.FirstTest2
                && this.machineVolatileDataProvider.Mode != MachineMode.FirstTest3
                && fastDeposit
                )
            {
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadingUnitId.Value);
                if (loadUnit.MaxNetWeight > 0 && loadUnit.GrossWeight > 0)
                {
                    if (loadUnit.GrossWeight < loadUnit.Tare)
                    {
                        scalingFactor = 0;
                    }
                    else
                    {
                        scalingFactor = (loadUnit.GrossWeight - loadUnit.Tare) / loadUnit.MaxNetWeight;
                    }
                }
            }
            foreach (var profileStep in profileSteps)
            {
                profileStep.ScaleMovementsByWeight(scalingFactor, horizontalAxis);
                profileStep.AdjustPositionByCenter(horizontalAxis, profileType, center);
            }

            // if direction is Forwards then height increments, otherwise it decrements
            var directionMultiplier = (direction == HorizontalMovementDirection.Forwards ? 1 : -1);

            var speed = profileSteps.Select(s => s.Speed).ToArray();
            var acceleration = profileSteps.Select(s => s.Acceleration).ToArray();
            var deceleration = profileSteps.Select(s => s.Acceleration).ToArray();

            // we use compensation for small errors only (large errors come from new database)
            var compensation = this.HorizontalPosition - horizontalAxis.LastIdealPosition;
            if (Math.Abs(compensation) > Math.Abs(horizontalAxis.ChainOffset))
            {
                this.logger.LogWarning($"Do not use compensation for large errors {compensation:0.00} > offset {horizontalAxis.ChainOffset}");
                compensation = 0;
            }
            var switchPosition = profileSteps.Select(s => this.HorizontalPosition - compensation + (s.Position * directionMultiplier)).ToArray();

            var targetPosition = switchPosition.Last();

            this.logger.LogInformation($"MoveHorizontalAuto: ProfileType: {profileType}; " +
                $"HorizontalPosition: {(int)this.HorizontalPosition}; " +
                $"direction: {direction}; " +
                $"measure: {measure}; " +
                $"waitContinue: {waitContinue}; " +
                $"loadUnitId: {loadingUnitId}; " +
                $"scalingFactor: {scalingFactor:0.0000}; " +
                $"compensation: {compensation:0.00}; ");

            // ---------------------------
            // Horizontal movement message
            var horizontalMovementMessageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.TableTarget,
                (measure ? MovementMode.PositionAndMeasureProfile : MovementMode.Position),
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction,
                waitContinue)
            {
                TargetCellId = targetCellId,
                TargetBayPositionId = targetBayPositionId,
                SourceCellId = sourceCellId,
                SourceBayPositionId = sourceBayPositionId,
            };

            if (loadingUnitId.HasValue)
            {
                horizontalMovementMessageData.LoadingUnitId = loadingUnitId;
            }

            // Get the displacement along vertical axis
            var grossWeight = 0.0d;
            if (loadingUnitId.HasValue)
            {
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadingUnitId.Value);
                grossWeight = loadUnit.GrossWeight;
            }
            var displacement = this.invertersProvider.ComputeDisplacement(this.VerticalPosition, grossWeight);
            displacement *= (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null) ? -1 : +1;
            this.logger.LogDebug($"Combined movement: Vertical displacement: {displacement:0.00} mm [targetPosition: {this.VerticalPosition + displacement:0.00} mm], gross weight load unit: {grossWeight:0.00} kg");

            var manualVerticalParameters = this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical);

            // Check this
            var movementVerticalParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical, isLoadingUnitOnBoard);

            // Calculate the speed according to the space and time (time referred to the horizontal movement: t = space/speed)
            var accelerationVertical = new[] { movementVerticalParameters.Acceleration };
            var decelerationVertical = new[] { movementVerticalParameters.Deceleration };

            var feedRate = manualVerticalParameters.FeedRateAfterZero;

            // Calculate time [s] to perform the horizontal movement
            var nItems = profileSteps.Count();
            var time = 0.0d;
            var lastPosTmp = this.HorizontalPosition;
            for (var i = 0; i < nItems; i++)
            {
                time += Math.Abs(switchPosition[i] - lastPosTmp) / speed[i];
                lastPosTmp = switchPosition[i];
            }

            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
            var delay = 0;
            if (isLoadingUnitOnBoard
                && verticalAxis.VerticalDepositCompensationDelay.HasValue
                && verticalAxis.VerticalDepositCompensationDelay.Value > 0
                && time > verticalAxis.VerticalDepositCompensationDelay.Value / 10.0 + 1)
            {
                time -= verticalAxis.VerticalDepositCompensationDelay.Value / 10.0;
                delay = verticalAxis.VerticalDepositCompensationDelay.Value * 100;
            }
            else if (!isLoadingUnitOnBoard
                && verticalAxis.VerticalPickupCompensationDelay.HasValue
                && verticalAxis.VerticalPickupCompensationDelay.Value > 0
                && time > verticalAxis.VerticalPickupCompensationDelay.Value / 10.0 + 1)
            {
                time -= verticalAxis.VerticalPickupCompensationDelay.Value / 10.0;
                delay = verticalAxis.VerticalPickupCompensationDelay.Value * 100;
            }

            const double MIN_SPEED_VALUE = 0.1d;
            var speedVertical = new[] { (displacement != 0.0d) ? Math.Abs(displacement) / time : MIN_SPEED_VALUE };  // time = space / speed [s]

            var switchPositionVertical = new[] { 0.0 };

            // -------------------------
            // Vertical movement message
            var verticalMovementMessageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                (this.VerticalPosition + displacement),
                speedVertical,
                accelerationVertical,
                decelerationVertical,
                switchPositionVertical,
                HorizontalMovementDirection.Forwards)
            {
                LoadingUnitId = this.elevatorDataProvider.GetLoadingUnitOnBoard()?.Id,
                FeedRate = (sender == MessageActor.AutomationService ? feedRate : 1),
                ComputeElongation = false,
                TargetBayPositionId = (isLoadingUnitOnBoard ? targetBayPositionId : sourceBayPositionId),
                TargetCellId = (isLoadingUnitOnBoard ? targetCellId : sourceCellId),
                WaitContinue = waitContinue,
                IsPickupMission = false,
                BypassConditions = true,
                DelayStart = delay,
            };

            this.logger.LogInformation($"MoveToVerticalPosition: {MovementMode.Position}; " +
                $"manualMovement: {false}; " +
                $"targetPosition: {this.VerticalPosition + displacement:0.00}; [displacement: {displacement:0.00}]; " +
                $"homing: {false}; " +
                $"waitContinue: {waitContinue}; " +
                $"feedRate: {(sender == MessageActor.AutomationService ? feedRate : 1)}; " +
                $"speed: {speedVertical[0]:0.00}; " +
                $"acceleration: {accelerationVertical[0]:0.00}; " +
                $"deceleration: {decelerationVertical[0]:0.00}; " +
                $"speed w/o feedRate: {movementVerticalParameters.Speed:0.00}; " +
                $"Load Unit {horizontalMovementMessageData.LoadingUnitId.GetValueOrDefault()}; " +
                $"Load Unit gross weight {grossWeight:0.00}; " +
                $"Bypass condition {verticalMovementMessageData.BypassConditions}; " +
                $"time to perform the movement: {time:0.000} s; " +
                $"delay start: {delay} ms; " +
                $"vertical current position: {this.VerticalPosition:0.00} mm");

            // --------------------------
            // Combined movements message
            var combinedMovementsMessageData = new CombinedMovementsMessageData(
                horizontalMovementMessageData,
                verticalMovementMessageData);

            // Publish message to execute the combined movements
            this.PublishCommand(
                combinedMovementsMessageData,
                $"Execute Combined Movements Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.CombinedMovements,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        /// <summary>
        /// Moves the horizontal chain of the elevator in manual mode to load or unload a LoadUnit for the machine of 1 Ton.
        /// It uses a combination of indipendently horizontal movement and vertical movement.
        /// </summary>
        private void MoveHorizontalManual_ForOneTonMachine(
            HorizontalMovementDirection direction,
            double distance,
            double verticalDisplacement,
            bool measure,
            int? loadingUnitId,
            int? positionId,
            bool bypassConditions,
            BayNumber requestingBay,
            MessageActor sender,
            bool highSpeed)
        {
            // horizontal axis
            var horizontalAxis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var horizontalTargetPosition = this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay]
                ? horizontalAxis.ManualMovements.TargetDistanceAfterZero.Value
                : horizontalAxis.ManualMovements.TargetDistance.Value;

            if (distance > 0)
            {
                horizontalTargetPosition = distance;
            }

            horizontalTargetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var horizontalSpeed = new[] { horizontalAxis.FullLoadMovement.Speed * (highSpeed ? horizontalAxis.ManualMovements.FeedRateAfterZero : horizontalAxis.ManualMovements.FeedRate) };
            var horizontalAcceleration = new[] { horizontalAxis.FullLoadMovement.Acceleration };
            var horizontalDeceleration = new[] { horizontalAxis.FullLoadMovement.Deceleration };
            var switchHorizontalPosition = new[] { 0.0 };

            // ---------------------------
            // Horizontal movement message
            var horizontalMovementMessageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                (measure ? MovementMode.PositionAndMeasureProfile : MovementMode.Position),
                horizontalTargetPosition,
                horizontalSpeed,
                horizontalAcceleration,
                horizontalDeceleration,
                switchHorizontalPosition,
                direction);

            if (loadingUnitId.HasValue)
            {
                horizontalMovementMessageData.LoadingUnitId = loadingUnitId;
                horizontalMovementMessageData.SourceBayPositionId = positionId;
            }
            horizontalMovementMessageData.BypassConditions = bypassConditions;

            // vertical axis
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            var verticalAcceleration = new[] { verticalAxis.FullLoadMovement.Acceleration };
            var verticalDeceleration = new[] { verticalAxis.FullLoadMovement.Deceleration };
            var switchVerticalPosition = new[] { 0.0 };

            // Get the displacement along vertical axis
            var grossWeight = 0.0d;
            if (loadingUnitId.HasValue)
            {
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadingUnitId.Value);
                grossWeight = loadUnit.GrossWeight;
            }

            this.logger.LogDebug($"Combined movement: Vertical displacement: {verticalDisplacement:0.00} mm [targetPosition: {this.VerticalPosition + verticalDisplacement:0.00} mm], weight load unit: {grossWeight:0.00} kg");

            var time = Math.Abs(horizontalTargetPosition) / horizontalSpeed[0];  // Calculate the time = space / speed [s]

            // Get the speed along vertical axis
            const double MIN_SPEED_VALUE = 0.1d;
            var verticalSpeed = new[] { (verticalDisplacement != 0.0d) ? Math.Abs(verticalDisplacement) / time : MIN_SPEED_VALUE };

            // -------------------------
            // Vertical movement message
            var verticalMovementMessageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
                verticalDisplacement,
                verticalSpeed,
                verticalAcceleration,
                verticalDeceleration,
                switchVerticalPosition,
                direction);
            verticalMovementMessageData.BypassConditions = true;

            // --------------------------
            // Combined movements message
            var messageData = new CombinedMovementsMessageData(
                horizontalMovementMessageData,
                verticalMovementMessageData);

            // Publish message to execute the combined movements
            this.PublishCommand(
                messageData,
                $"Execute Combined Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.CombinedMovements,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        #endregion
    }
}
