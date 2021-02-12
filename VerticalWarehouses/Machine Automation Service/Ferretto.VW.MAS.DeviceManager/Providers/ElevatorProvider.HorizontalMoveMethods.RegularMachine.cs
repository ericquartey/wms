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
        /// Moves the horizontal chain of the elevator to load or unload a LoadUnit for the all machines, but the 1 Ton.
        /// </summary>
        private void MoveHorizontalAuto_ForRegularMachine(HorizontalMovementDirection direction,
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

            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var center = axis.Center;

            var profileSteps = axis.Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);

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
                profileStep.ScaleMovementsByWeight(scalingFactor, axis);
                profileStep.AdjustPositionByCenter(axis, profileType, center);
            }

            // if direction is Forwards then height increments, otherwise it decrements
            var directionMultiplier = (direction == HorizontalMovementDirection.Forwards ? 1 : -1);

            var speed = profileSteps.Select(s => s.Speed).ToArray();
            var acceleration = profileSteps.Select(s => s.Acceleration).ToArray();
            var deceleration = profileSteps.Select(s => s.Acceleration).ToArray();

            // we use compensation for small errors only (large errors come from new database)
            var compensation = this.HorizontalPosition - axis.LastIdealPosition;
            if (Math.Abs(compensation) > Math.Abs(axis.ChainOffset))
            {
                this.logger.LogWarning($"Do not use compensation for large errors {compensation:0.00} > offset {axis.ChainOffset}");
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
                $"compensation: {compensation:0.00}");

            var messageData = new PositioningMessageData(
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
                messageData.LoadingUnitId = loadingUnitId;
            }

            // Publish the command to execute the movement
            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        /// <summary>
        /// Moves the horizontal chain of the elevator in manual mode to load or unload a LoadUnit for the all machines, but the 1 Ton.
        /// </summary>
        private void MoveHorizontalManual_ForRegularMachine(
            HorizontalMovementDirection direction,
            double distance,
            bool measure,
            int? loadingUnitId,
            int? positionId,
            bool bypassConditions,
            BayNumber requestingBay,
            MessageActor sender,
            bool highSpeed)
        {
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var targetPosition = this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay]
                ? axis.ManualMovements.TargetDistanceAfterZero.Value
                : axis.ManualMovements.TargetDistance.Value;
            if (distance > 0)
            {
                targetPosition = distance;
            }

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = new[] { axis.FullLoadMovement.Speed * (highSpeed ? axis.ManualMovements.FeedRateAfterZero : axis.ManualMovements.FeedRate) };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                (measure ? MovementMode.PositionAndMeasureProfile : MovementMode.Position),
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction);

            if (loadingUnitId.HasValue)
            {
                messageData.LoadingUnitId = loadingUnitId;
                messageData.SourceBayPositionId = positionId;
            }
            messageData.BypassConditions = bypassConditions;

            // Publish message to execute the horizontal movement
            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        #endregion
    }
}
