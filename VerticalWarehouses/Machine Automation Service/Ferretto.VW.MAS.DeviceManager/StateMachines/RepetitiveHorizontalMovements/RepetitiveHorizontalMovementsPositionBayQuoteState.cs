using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class RepetitiveHorizontalMovementsPositionBayQuoteState : StateBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IRepetitiveHorizontalMovementsMachineData machineData;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServiceScope scope;

        private readonly ISensorsProvider sensorsProvider;

        private readonly IRepetitiveHorizontalMovementsStateData stateData;

        private InternalPreviousStates _previousState;

        #endregion

        #region Constructors

        public RepetitiveHorizontalMovementsPositionBayQuoteState(IRepetitiveHorizontalMovementsStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IRepetitiveHorizontalMovementsMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

            this.machineResourcesProvider = this.scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
            this.loadingUnitsDataProvider = this.scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            this.sensorsProvider = this.scope.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.machineVolatileDataProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();

            this._previousState = InternalPreviousStates.None;
        }

        #endregion

        #region Enums

        // Only for Internal use
        private enum InternalPreviousStates
        {
            None = -1,

            OnElevator = 0,

            OnBay = 1
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            switch (message.Type)
            {
                case MessageType.StopTest:
                    this.Logger.LogInformation($"Stop Test on {this.machineData.RequestingBay} after {this.machineData.MessageData.ExecutedCycles} movements");
                    this.machineData.MessageData.IsTestStopped = true;
                    break;

                default:
                    break;
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source}");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogDebug($"1:Process Notitication Message {message.Type} Source {message.Source}");

            if (MessageType.Positioning == message.Type)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        {
                            if (this._previousState == InternalPreviousStates.OnElevator)
                            {
                                // A transaction to update elevatorData provider & baysData provider
                                var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
                                using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                                {
                                    this.elevatorDataProvider.SetLoadingUnit(null);
                                    this.baysDataProvider.SetLoadingUnit(bayPosition.Id, this.machineData.MessageData.LoadingUnitId);

                                    transaction.Commit();
                                }

                                this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsOnBayState(this.stateData, this.Logger));
                            }

                            if (this._previousState == InternalPreviousStates.OnBay)
                            {
                                // A transaction to update elevatorData provider & baysData provider
                                var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
                                using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                                {
                                    this.elevatorDataProvider.SetLoadingUnit(this.machineData.MessageData.LoadingUnitId);
                                    this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);

                                    transaction.Commit();
                                }

                                this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsOnElevatorState(this.stateData, this.Logger));
                            }

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

            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            var bay = this.baysDataProvider.GetByNumber(this.machineData.TargetBay);

            // Check if a loading unit is on board of the elevator to detect the previous state
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null)
            {
                this._previousState = InternalPreviousStates.OnElevator;

                // Check if drawer can be unloaded to bay
                var policy = this.CanUnloadToBay(bayPosition.Id, this.machineData.TargetBay);
                if (!policy.IsAllowed)
                {
                    this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsErrorState(this.stateData, this.Logger));
                    return;
                }

                var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();

                var direction = bay.Side == WarehouseSide.Front
                    ? HorizontalMovementDirection.Backwards
                    : HorizontalMovementDirection.Forwards;

                var loadingUnitGrossWeight = (this.machineData.AcquiredWeight) ?
                    loadingUnit.GrossWeight :
                    0.0d;

                this.MoveHorizontalAuto(
                    direction,
                    isLoadingUnitOnBoard: true,
                    loadingUnit.Id,
                    loadingUnitGrossWeight,
                    waitContinue: false,
                    measure: false,
                    this.machineData.RequestingBay,
                    MessageActor.DeviceManager,
                    targetBayPositionId: bayPosition?.Id);
            }
            else
            {
                this._previousState = InternalPreviousStates.OnBay;

                // Check if drawer can be loaded from bay
                var policy = this.CanLoadFromBay(bayPosition.Id, this.machineData.TargetBay);
                if (!policy.IsAllowed)
                {
                    this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsErrorState(this.stateData, this.Logger));
                    return;
                }

                var direction = bay.Side is WarehouseSide.Front
                    ? HorizontalMovementDirection.Forwards
                    : HorizontalMovementDirection.Backwards;

                var loadingUnitGrossWeight = (!this.machineData.AcquiredWeight) ?
                    bayPosition.LoadingUnit.MaxNetWeight + bayPosition.LoadingUnit.Tare :
                    bayPosition.LoadingUnit.GrossWeight;

                this.MoveHorizontalAuto(
                    direction,
                    isLoadingUnitOnBoard: false,
                    bayPosition.LoadingUnit.Id,
                    loadingUnitGrossWeight,
                    waitContinue: false,
                    measure: false,
                    this.machineData.RequestingBay,
                    MessageActor.DeviceManager,
                    sourceBayPositionId: bayPosition?.Id);
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsEndState(this.stateData, this.Logger));
        }

        /// <summary>
        /// Check if drawer can be loaded from bay
        /// </summary>
        /// <param name="bayPositionId">The bay position Id</param>
        /// <param name="bayNumber">The requesting bay</param>
        /// <returns></returns>
        private ActionPolicy CanLoadFromBay(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified bay position
            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            if (bayPosition?.Id != bayPositionId)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotLocatedOppositeToTheSpecifiedBayPosition", CommonUtils.Culture.Actual)
                };
            }

            // check #2: a loading unit must be present in the bay position
            if (!this.IsBayPositionOccupied(bayNumber, bayPositionId))
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("NoLoadingUnitIsPresentInTheSpecifiedBayPosition", CommonUtils.Culture.Actual)
                };
            }

            // check #5: elevator's pawl must be in zero position
            if (!this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual)
                };
            }

            return ActionPolicy.Allowed;
        }

        /// <summary>
        /// Check if drawer can be unloaded to bay.
        /// </summary>
        /// <param name="bayPositionId">The bay position Id</param>
        /// <param name="bayNumber">The requesting bay</param>
        /// <returns></returns>
        private ActionPolicy CanUnloadToBay(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified bay position
            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            if (bayPosition?.Id != bayPositionId)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotLocatedOppositeToTheSpecifiedBayPosition", CommonUtils.Culture.Actual)
                };
            }

            // check #2: the bay position must not contain a loading unit
            if (!this.IsBayPositionEmpty(bayNumber, bayPositionId))
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("ALoadingUnitIsAlreadyPresentInTheSpecifiedBayPosition", CommonUtils.Culture.Actual)
                };
            }

            // check #5: elevator's pawl cannot be be in zero position
            if (this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual) };
            }

            return ActionPolicy.Allowed;
        }

        /// <summary>
        /// Check if bay position is empty
        /// </summary>
        /// <param name="bayNumber">The requesting bay</param>
        /// <param name="bayPositionId">The bay position Id</param>
        /// <returns></returns>
        private bool IsBayPositionEmpty(BayNumber bayNumber, int bayPositionId)
        {
            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);

            var arePresenceSensorsActive = bayPosition.IsUpper
                ? this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                : this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber);

            return bayPosition.LoadingUnit is null && !arePresenceSensorsActive;
        }

        /// <summary>
        /// Check if bay position is full.
        /// </summary>
        /// <param name="bayNumber">The requesting bay</param>
        /// <param name="bayPositionId">The bay position Id</param>
        /// <returns></returns>
        private bool IsBayPositionOccupied(BayNumber bayNumber, int bayPositionId)
        {
            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);

            var arePresenceSensorsActive = bayPosition.IsUpper
                ? this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                : this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber);

            return bayPosition.LoadingUnit != null && arePresenceSensorsActive;
        }

        /// <summary>
        /// Moves the horizontal chain of the elevator to load or unload a LoadUnit.
        /// It uses a Table target movement, mapped by 4 Profiles sets of parameters selected by direction and loading status
        /// </summary>
        /// <param name="direction">Forwards: from elevator to Bay 1 side</param>
        /// <param name="isLoadingUnitOnBoard">true: elevator is full before the movement. It must match the presence sensors</param>
        /// <param name="loadingUnitId">This id is stored in Elevator table before the movement. null means no LoadUnit</param>
        /// <param name="loadingUnitGrossWeight">This weight is stored in LoadingUnits table before the movement.</param>
        /// <param name="waitContinue">true: the inverter positioning state machine stops after the transmission of parameters and waits for a Continue command before enabling inverter
        ///                             the scope is to wait for the shutter to open or close before moving </param>
        /// <param name="requestingBay">The requesting bay for the current operation</param>
        /// <param name="sender">The message actor</param>
        private void MoveHorizontalAuto(
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
            int? sourceBayPositionId = null)
        {
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

            var profileType = this.elevatorProvider.SelectProfileType(direction, isLoadingUnitOnBoard);

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
            var compensation = this.elevatorProvider.HorizontalPosition - axis.LastIdealPosition;
            if (Math.Abs(compensation) > Math.Abs(axis.ChainOffset))
            {
                this.Logger.LogWarning($"Do not use compensation for large errors {compensation:0.00} > offset {axis.ChainOffset}");
                compensation = 0;
            }
            var switchPosition = profileSteps.Select(s => this.elevatorProvider.HorizontalPosition - compensation + (s.Position * directionMultiplier)).ToArray();

            var targetPosition = switchPosition.Last();

            this.Logger.LogInformation($"MoveHorizontalAuto: ProfileType: {profileType}; " +
                $"HorizontalPosition: {(int)this.elevatorProvider.HorizontalPosition}; " +
                $"direction: {direction}; " +
                $"measure: {measure}; " +
                $"waitContinue: {waitContinue}; " +
                $"loadUnitId: {loadingUnitId}; " +
                $"scalingFactor: {scalingFactor:0.0000}; " +
                $"compensation: {compensation:0.00}");

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.TableTarget,
                MovementMode.Position,
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

            var message = new CommandMessage(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
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
