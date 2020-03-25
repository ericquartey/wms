using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//
// TO REMOVE
//

namespace Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements
{
    internal class RepetitiveHorizontalMovementsStartOldState : StateBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IRepetitiveHorizontalMovementsMachineData machineData;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServiceScope scope;

        private readonly ISensorsProvider sensorsProvider;

        private readonly IRepetitiveHorizontalMovementsStateData stateData;

        #endregion

        #region Constructors

        public RepetitiveHorizontalMovementsStartOldState(IRepetitiveHorizontalMovementsStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IRepetitiveHorizontalMovementsMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.loadingUnitsDataProvider = this.scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();

            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
            this.sensorsProvider = this.scope.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.machineVolatileDataProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
            //x throw new NotImplementedException();
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source}");
            //x throw new NotImplementedException();
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            if (message.Type == MessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        {
                            // the height is acquired
                            //this.machineData.AcquiredHeightProfile = true;

                            this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsOnElevatorState(this.stateData));
                            break;
                        }
                    case MessageStatus.OperationError:
                        {
                            this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsErrorState(this.stateData));
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
            // update the ExecutedCycles/PerformedCycles before start the test
            //using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
            //{
            // use the ISetupProceduresDataProvider interface
            //this.machineData.MessageData.ExecutedCycles = scope.ServiceProvider
            //        .GetRequiredService<ISetupProceduresDataProvider>()
            //        .GetRepetitiveHorizontalMovementsTest()
            //        .PerformedCycles;
            this.machineData.MessageData.ExecutedCycles = 0;

            //this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Test;
            this.machineVolatileDataProvider.Mode = MachineMode.Test;
            this.Logger.LogInformation($"Machine status switched to {MachineMode.Test}");
            //}

            // See LoadFromBay() at ElevatorProvider interface
            // MoveHorizontalAuto(
            //      Axis.Horizontal,
            //      MovementType.TableTarget,
            //      MovementMode.PositionAndMeasureProfile if !this.ParentState.AcquiredHeightProfile, MovementMode.Position otherwise,
            //      .... )
            //

            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            var bay = this.baysDataProvider.GetByNumber(this.machineData.TargetBay);

            var direction = bay.Side is WarehouseSide.Front
                ? HorizontalMovementDirection.Backwards
                : HorizontalMovementDirection.Forwards;

            var supposedLoadingUnitGrossWeight = bayPosition.LoadingUnit.MaxNetWeight + bayPosition.LoadingUnit.Tare;

            this.MoveHorizontalAuto(
                direction,
                isLoadingUnitOnBoard: false,
                bayPosition.LoadingUnit.Id,
                supposedLoadingUnitGrossWeight,
                waitContinue: false,
                measure: true,
                /*bayNumber*/ this.machineData.TargetBay,
                /*sender*/ MessageActor.DeviceManager,
                sourceBayPositionId: /*bayPositionId*/ bayPosition?.Id);

            //x throw new NotImplementedException();
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new RepetitiveHorizontalMovementsEndState(this.stateData));
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
        /// <param name="requestingBay"></param>
        /// <param name="sender"></param>
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
            if (loadingUnitId.HasValue && loadingUnitGrossWeight.HasValue)
            {
                this.loadingUnitsDataProvider.SetWeight(loadingUnitId.Value, loadingUnitGrossWeight.Value);
            }

            var sensors = this.sensorsProvider.GetAll();

            var zeroSensor = this.machineVolatileDataProvider.IsOneTonMachine.Value
                ? IOMachineSensors.ZeroPawlSensorOneTon
                : IOMachineSensors.ZeroPawlSensor;

            if (!isLoadingUnitOnBoard && !sensors[(int)zeroSensor])
            {
                throw new InvalidOperationException(Resources.Elevator.TheElevatorIsNotFullButThePawlIsNotInZeroPosition);
            }
            if (isLoadingUnitOnBoard && sensors[(int)zeroSensor])
            {
                throw new InvalidOperationException(Resources.Elevator.TheElevatorIsNotEmptyButThePawlIsInZeroPosition);
            }

            //if (measure && isLoadingUnitOnBoard)
            //{
            //    this.Logger.LogWarning($"Do not measure profile on full elevator!");
            //    measure = false;
            //}

            var profileType = this.elevatorProvider.SelectProfileType(direction, isLoadingUnitOnBoard);

            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var profileSteps = axis.Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);

            //if (!loadingUnitId.HasValue && isLoadingUnitOnBoard)
            //{
            //    var loadUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            //    if (loadUnit != null)
            //    {
            //        loadingUnitId = loadUnit.Id;
            //    }
            //}

            // if weight is unknown we move as full weight
            double scalingFactor = 1;
            //if (loadingUnitId.HasValue
            //    && !measure
            //    && this.machineVolatileDataProvider.Mode != MachineMode.FirstTest
            //    )
            //{
            //    var loadUnit = this.loadingUnitsDataProvider.GetById(loadingUnitId.Value);
            //    if (loadUnit.MaxNetWeight > 0 && loadUnit.GrossWeight > 0)
            //    {
            //        if (loadUnit.GrossWeight < loadUnit.Tare)
            //        {
            //            scalingFactor = 0;
            //        }
            //        else
            //        {
            //            scalingFactor = (loadUnit.GrossWeight - loadUnit.Tare) / loadUnit.MaxNetWeight;
            //        }
            //    }
            //}

            foreach (var profileStep in profileSteps)
            {
                profileStep.ScaleMovementsByWeight(scalingFactor, axis);
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
                this.Logger.LogWarning($"Do not use compensation for large errors {compensation:0.2} > offset {axis.ChainOffset}");
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
                $"scalingFactor: {scalingFactor:0.4}; " +
                $"compensation: {compensation:0.2}");

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.TableTarget,
                //(measure ? MovementMode.PositionAndMeasureProfile : MovementMode.Position),
                MovementMode.PositionAndMeasureProfile,
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

            var commandMessage = new CommandMessage(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);

            this.ParentStateMachine.PublishCommandMessage(commandMessage);
        }

        #endregion
    }
}
