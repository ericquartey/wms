﻿using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningEndState : StateBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitProvider;

        private readonly IPositioningMachineData machineData;

        private readonly double minHeight = 25.0;

        private readonly IServiceScope scope;

        private readonly IPositioningStateData stateData;

        #endregion

        #region Constructors

        public PositioningEndState(IPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.loadingUnitProvider = this.scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            switch (message.Type)
            {
                case FieldMessageType.InverterStop:
                case FieldMessageType.Positioning:
                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterSwitchOff:
                    if (message.DeviceIndex != (byte)this.machineData.CurrentInverterIndex)
                    {
                        break;
                    }
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:

                            if (message.Status is MessageStatus.OperationEnd
                                && (this.machineData.MessageData.AxisMovement is Axis.Horizontal
                                    || this.machineData.MessageData.AxisMovement is Axis.BayChain)
                                )
                            {
                                this.UpdateLoadingUnitLocation();
                            }

                            var notificationMessage = new NotificationMessage(
                                this.machineData.MessageData,
                                this.machineData.MessageData.RequiredCycles == 0 ? "Positioning Stopped" : "Test Stopped",
                                MessageActor.DeviceManager,
                                MessageActor.DeviceManager,
                                MessageType.Positioning,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                StopRequestReasonConverter.GetMessageStatusFromReason(StopRequestReason.Stop));

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.RequestingBay);
                            this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData, this.Logger));
                            break;
                    }
                    break;

                case FieldMessageType.MeasureProfile:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationEnd:
                            if (message.Data is MeasureProfileFieldMessageData data && message.Source == FieldMessageActor.InverterDriver)
                            {
                                var profileHeight = this.baysDataProvider.ConvertProfileToHeight(data.Profile, this.machineData.MessageData.SourceBayPositionId.Value);
                                this.Logger.LogInformation($"Height measured {profileHeight}mm. Profile {data.Profile / 100.0}%");
                                if ((profileHeight < this.minHeight - 2.5) || data.Profile > 10000)
                                {
                                    this.Logger.LogError($"Measure Profile error {profileHeight}!");
                                    break;
                                }
                                var loadUnitId = this.machineData.MessageData.LoadingUnitId;
                                if (!loadUnitId.HasValue)
                                {
                                    var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
                                    var loadingUnitOnElevator = this.elevatorDataProvider.GetLoadingUnitOnBoard();
                                    if (bayPosition != null
                                        && bayPosition.LoadingUnit != null
                                        && loadingUnitOnElevator is null
                                        )
                                    {
                                        // manual pickup from bay
                                        loadUnitId = bayPosition.LoadingUnit.Id;
                                    }
                                }
                                if (loadUnitId.HasValue)
                                {
                                    this.loadingUnitProvider.SetHeight(loadUnitId.Value, profileHeight);
                                }
                                this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                            }
                            else if (message.Source == FieldMessageActor.IoDriver)
                            {
                                // we send the first request to read the height only after IoDriver has reset the reading enable signal
                                this.RequestMeasureProfile();
                            }
                            break;

                        case MessageStatus.OperationError:
                            this.stateData.FieldMessage = message;
                            this.Logger.LogError($"Measure Profile OperationError!");
                            //this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                            break;
                    }
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex} StopRequestReason {this.stateData.StopRequestReason}");
            if (this.machineData.MessageData.AxisMovement is Axis.Horizontal
                || this.machineData.MessageData.AxisMovement is Axis.BayChain
                )
            {
                this.UpdateLoadingUnitLocation();
            }

            if (this.machineData.MessageData.AxisMovement is Axis.Vertical)
            {
                this.PersistElevatorPosition(
                    this.machineData.MessageData.TargetBayPositionId,
                    this.machineData.MessageData.TargetCellId,
                    this.elevatorProvider.VerticalPosition);
            }

            var inverterIndex = this.machineData.CurrentInverterIndex;
            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                var stopMessage = new FieldCommandMessage(
                    null,
                    this.machineData.MessageData.RequiredCycles == 0 ? "Positioning Stopped" : "Test Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterStop,
                    (byte)inverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                if (this.machineData.MessageData.AxisMovement is Axis.Horizontal &&
                        this.machineData.MessageData.MovementType == MovementType.TableTarget)
                {
                    this.UpdateLastIdealPosition(this.machineData.MessageData.AxisMovement);
                }
                else if (this.machineData.MessageData.AxisMovement is Axis.BayChain
                    && this.machineData.MessageData.MovementMode == MovementMode.BayChain)
                {
                    this.UpdateLastIdealPosition(this.machineData.MessageData.AxisMovement);
                }

                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    this.machineData.MessageData.RequiredCycles == 0 ? "Positioning Completed" : "Test Completed",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Positioning End");
            }

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, SENSOR_UPDATE_SLOW);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"4:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            if (this.machineData.MessageData.MovementMode == MovementMode.BeltBurnishing
                || this.machineData.MessageData.MovementMode == MovementMode.BayTest
                )
            {
                this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual;
                this.Logger.LogInformation($"Machine status switched to {MachineMode.Manual}");
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("Retry Stop Command");
            this.Start();
        }

        private void PersistElevatorPosition(int? targetBayPositionId, int? targetCellId, double targetPosition)
        {
            using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
            {
                var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

                var previousCell = this.elevatorDataProvider.GetCachedCurrentCell();
                var previousBayPosition = this.elevatorDataProvider.GetCachedCurrentBayPosition();

                using (var transaction = elevatorDataProvider.GetContextTransaction())
                {
                    if ((previousCell?.Id != targetCellId) || (previousBayPosition?.Id != targetBayPositionId))
                    {
                        // se uno dei due target è null e il suo previous non è null e se è dentro la tolleranza non setto null
                        if ((targetBayPositionId != null && targetCellId != null) ||
                            (previousBayPosition == null && previousCell == null) ||
                            (targetCellId == null && previousCell != null && !elevatorDataProvider.IsVerticalPositionWithinTolerance(previousCell.Position)) ||
                            (targetBayPositionId == null && previousBayPosition != null && !elevatorDataProvider.IsVerticalPositionWithinTolerance(previousBayPosition.Height)) ||
                            (targetCellId != null && previousCell != null && targetCellId != previousCell.Id) ||
                            (targetBayPositionId != null && previousBayPosition != null && targetBayPositionId != previousBayPosition.Id)
                            )
                        {
                            elevatorDataProvider.SetCurrentBayPosition(targetBayPositionId);
                            elevatorDataProvider.SetCurrentCell(targetCellId);
                            elevatorDataProvider.UpdateLastIdealPosition(targetPosition, Orientation.Vertical);
                        }
                    }
                    else
                    {
                        if ((previousCell != null && !elevatorDataProvider.IsVerticalPositionWithinTolerance(previousCell.Position)) ||
                            (previousBayPosition != null && !elevatorDataProvider.IsVerticalPositionWithinTolerance(previousBayPosition.Height)))
                        {
                            elevatorDataProvider.SetCurrentBayPosition(null);
                            elevatorDataProvider.SetCurrentCell(null);
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        private void RequestMeasureProfile()
        {
            this.Logger.LogDebug($"Request MeasureProfile ");
            var inverterIndex = this.baysDataProvider.GetInverterIndexByProfile(this.machineData.RequestingBay);

            var inverterCommandMessageData = new MeasureProfileFieldMessageData();
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Measure Profile",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.MeasureProfile,
                (byte)inverterIndex);

            this.Logger.LogTrace($"5:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);
        }

        private void UpdateLastIdealPosition(Axis axis)
        {
            var serviceProvider = this.ParentStateMachine.ServiceScopeFactory.CreateScope().ServiceProvider;
            if (axis == Axis.Horizontal)
            {
                var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
                elevatorDataProvider.UpdateLastIdealPosition(this.machineData.MessageData.TargetPosition);
            }
            else if (axis == Axis.BayChain)
            {
                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                var position = baysDataProvider.GetChainPosition(this.machineData.RequestingBay);
                baysDataProvider.UpdateLastIdealPosition(position, this.machineData.RequestingBay);
            }
        }

        private void UpdateLoadingUnitForManualMovement()
        {
            using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
            {
                var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                var cellsProvider = scope.ServiceProvider.GetRequiredService<ICellsProvider>();
                var machineResourcesProvider = scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
                var loadingUnitProvider = scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();

                var loadingUnitOnElevator = elevatorDataProvider.GetLoadingUnitOnBoard();

                // 1. check if elevator is opposite a bay or a cell
                var bayPosition = elevatorDataProvider.GetCurrentBayPosition();
                var cell = elevatorDataProvider.GetCurrentCell();

                var isChanged = false;
                using (var transaction = elevatorDataProvider.GetContextTransaction())
                {
                    if (this.machineData.MessageData.AxisMovement is Axis.BayChain)
                    {
                        var bay = baysDataProvider.GetByNumber(this.machineData.RequestingBay);
                        if (this.machineData.MessageData.TargetPosition > 0
                            && machineResourcesProvider.IsDrawerInBayTop(bay.Number)
                            && !machineResourcesProvider.IsDrawerInBayBottom(bay.Number))
                        {
                            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                            var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                            if (origin != null
                                && destination != null
                                && origin.LoadingUnit != null)
                            {
                                baysDataProvider.SetLoadingUnit(destination.Id, origin.LoadingUnit.Id, 0);
                                baysDataProvider.SetLoadingUnit(origin.Id, null);
                                isChanged = true;
                            }
                        }
                    }
                    else if (bayPosition != null)
                    {
                        var bay = baysDataProvider.GetByBayPositionId(bayPosition.Id);
                        var isDrawerInBay = bayPosition.IsUpper
                             ? machineResourcesProvider.IsDrawerInBayTop(bay.Number)
                             : machineResourcesProvider.IsDrawerInBayBottom(bay.Number);

                        if (loadingUnitOnElevator == null && bayPosition.LoadingUnit != null)
                        // possible pickup from bay
                        {
                            if (machineResourcesProvider.IsDrawerCompletelyOnCradle && !isDrawerInBay)
                            {
                                elevatorDataProvider.SetLoadingUnit(bayPosition.LoadingUnit.Id);
                                baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                                isChanged = true;
                            }
                        }
                        else if (loadingUnitOnElevator != null && bayPosition.LoadingUnit == null)
                        // possible deposit to bay
                        {
                            if (machineResourcesProvider.IsDrawerCompletelyOffCradle && isDrawerInBay)
                            {
                                elevatorDataProvider.SetLoadingUnit(null);
                                baysDataProvider.SetLoadingUnit(bayPosition.Id, loadingUnitOnElevator.Id);
                                loadingUnitProvider.SetHeight(loadingUnitOnElevator.Id, 0);
                                isChanged = true;
                            }
                        }
                    }
                    else if (cell != null)
                    {
                        if (loadingUnitOnElevator == null && cell.LoadingUnit != null)
                        // possible pickup from cell
                        {
                            if (machineResourcesProvider.IsDrawerCompletelyOnCradle)
                            {
                                elevatorDataProvider.SetLoadingUnit(cell.LoadingUnit.Id);
                                cellsProvider.SetLoadingUnit(cell.Id, null);
                                isChanged = true;
                            }
                        }
                        else if (loadingUnitOnElevator != null && cell.LoadingUnit == null)
                        // possible deposit to cell
                        {
                            if (machineResourcesProvider.IsDrawerCompletelyOffCradle)
                            {
                                if (cellsProvider.CanFitLoadingUnit(cell.Id, loadingUnitOnElevator.Id))
                                {
                                    elevatorDataProvider.SetLoadingUnit(null);
                                    cellsProvider.SetLoadingUnit(cell.Id, loadingUnitOnElevator.Id);
                                    isChanged = true;
                                }
                                else
                                {
                                    this.Logger.LogWarning("Detected loading unit leaving the cradle, but cell cannot store it.");
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                if (isChanged)
                {
                    this.ParentStateMachine.PublishNotificationMessage(
                            new NotificationMessage(
                                null,
                                $"Load Unit position changed",
                                MessageActor.Any,
                                MessageActor.DeviceManager,
                                MessageType.Positioning,
                                this.stateData.MachineData.RequestingBay,
                                this.stateData.MachineData.RequestingBay,
                                MessageStatus.OperationUpdateData));
                }
            }
        }

        private void UpdateLoadingUnitLocation()
        {
            if (!this.machineData.MessageData.LoadingUnitId.HasValue)
            {
                this.UpdateLoadingUnitForManualMovement();
            }
        }

        #endregion
    }
}
