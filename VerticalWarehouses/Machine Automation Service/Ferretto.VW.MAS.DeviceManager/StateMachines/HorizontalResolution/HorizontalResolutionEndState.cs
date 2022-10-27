using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.HorizontalResolution.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.HorizontalResolution
{
    internal class HorizontalResolutionEndState : StateBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IHorizontalResolutionMachineData machineData;

        //private readonly double minHeight = 25.0;

        private readonly IServiceScope scope;

        private readonly IHorizontalResolutionStateData stateData;

        #endregion

        #region Constructors

        public HorizontalResolutionEndState(IHorizontalResolutionStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHorizontalResolutionMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
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

                            var notificationMessage = new NotificationMessage(
                                this.machineData.MessageData,
                                this.machineData.MessageData.RequiredCycles == 0 ? "Horizontal Resolution Stopped" : "Test Stopped",
                                MessageActor.DeviceManager,
                                MessageActor.DeviceManager,
                                MessageType.Positioning,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                StopRequestReasonConverter.GetMessageStatusFromReason(StopRequestReason.Stop));

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.TargetBay);
                            this.ParentStateMachine.ChangeState(new HorizontalResolutionErrorState(this.stateData, this.Logger));
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

            //if (this.machineData.MessageData.AxisMovement is Axis.Vertical)
            //{
            //    this.PersistElevatorPosition(
            //        this.machineData.MessageData.TargetBayPositionId,
            //        this.machineData.MessageData.TargetCellId,
            //        this.machineData.MessageData.TargetPosition);
            //}

            var inverterIndex = this.machineData.CurrentInverterIndex;
            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                var stopMessage = new FieldCommandMessage(
                    null,
                    this.machineData.MessageData.RequiredCycles == 0 ? "Horizontal Resolution Stopped" : "Test Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterStop,
                    (byte)inverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                if (this.machineData.MessageData.AxisMovement is Axis.Horizontal)
                {
                    this.UpdateLastIdealPosition(this.machineData.MessageData.AxisMovement);
                }

                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    this.machineData.MessageData.RequiredCycles == 0 ? "Horizontal Resolution Completed" : "Test Completed",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Horizontal Resolution End");
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

            if (this.machineData.MessageData.MovementMode == MovementMode.HorizontalResolution)
            {
                //this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual;
                //this.Logger.LogInformation($"Machine status switched to {MachineMode.Manual}");
                switch (this.machineData.TargetBay)
                {
                    case BayNumber.BayOne:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual;
                        break;

                    case BayNumber.BayTwo:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual2;
                        break;

                    case BayNumber.BayThree:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual3;
                        break;

                    default:
                        this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode = MachineMode.Manual;
                        break;
                }

                this.Logger.LogInformation($"Machine status switched to {this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>().Mode}");
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("Retry Stop Command");
            this.Start();
        }

        private void UpdateLastIdealPosition(Axis axis)
        {
            var serviceProvider = this.ParentStateMachine.ServiceScopeFactory.CreateScope().ServiceProvider;
            if (axis == Axis.Horizontal)
            {
                var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
                elevatorDataProvider.UpdateLastIdealPosition(this.machineData.MessageData.TargetPosition);
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

                //
                // NOT Update the loading unit position:
                //   LU is always in the bay
                //   the position can changed: internal position or external position (notify by the sensors)
                //

                // USELESS ===>
                // 1. check if elevator is opposite a bay or a cell
                var bayPosition = elevatorDataProvider.GetCurrentBayPosition();
                var cell = elevatorDataProvider.GetCurrentCell();

                //var bay = baysDataProvider.GetByNumber(this.machineData.RequestingBay);

                var isChanged = false;
                using (var transaction = elevatorDataProvider.GetContextTransaction())
                {
                    if (this.machineData.MessageData.AxisMovement is Axis.BayChain)
                    {
                        // TODO:
                        // Define a baysDataProvider.SetLoadingUnit(*) for the position internal and external (?)

                        //var bay = baysDataProvider.GetByNumber(this.machineData.RequestingBay);

                        //if (loadingUnitOnElevator == null && bayPosition.LoadingUnit != null)
                        //{
                        //    // 1. Possible pickup from bay
                        //    if (machineResourcesProvider.IsDrawerCompletelyOnCradle &&
                        //        !machineResourcesProvider.IsDrawerInBayInternalPosition(bay.Number) &&
                        //        !machineResourcesProvider.IsDrawerInBayExternalPosition(bay.Number))
                        //    {
                        //        elevatorDataProvider.SetLoadingUnit(bayPosition.LoadingUnit.Id);
                        //        baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                        //        isChanged = true;
                        //    }
                        //}
                        //else if (loadingUnitOnElevator != null && bayPosition.LoadingUnit == null)
                        //{
                        //    // 2. Possible deposit to bay
                        //    if (machineResourcesProvider.IsDrawerCompletelyOffCradle &&
                        //        machineResourcesProvider.IsDrawerInBayInternalPosition(bay.Number) &&
                        //        !machineResourcesProvider.IsDrawerInBayExternalPosition(bay.Number))
                        //    {
                        //        elevatorDataProvider.SetLoadingUnit(null);
                        //        baysDataProvider.SetLoadingUnit(bayPosition.Id, loadingUnitOnElevator.Id);
                        //        loadingUnitProvider.SetHeight(loadingUnitOnElevator.Id, 0);
                        //        isChanged = true;
                        //    }
                        //}
                    }

                    //if (this.machineData.MessageData.AxisMovement is Axis.BayChain)
                    //{
                    //    var bay = baysDataProvider.GetByNumber(this.machineData.RequestingBay);
                    //    if (this.machineData.MessageData.TargetPosition > 0
                    //        && machineResourcesProvider.IsDrawerInBayTop(bay.Number)
                    //        && !machineResourcesProvider.IsDrawerInBayBottom(bay.Number))
                    //    {
                    //        var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                    //        var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                    //        if (origin != null
                    //            && destination != null
                    //            && origin.LoadingUnit != null)
                    //        {
                    //            baysDataProvider.SetLoadingUnit(destination.Id, origin.LoadingUnit.Id, 0);
                    //            baysDataProvider.SetLoadingUnit(origin.Id, null);
                    //            isChanged = true;
                    //        }
                    //    }
                    //}
                    //else if (bayPosition != null)
                    //{
                    //    var bay = baysDataProvider.GetByBayPositionId(bayPosition.Id);
                    //    var isDrawerInBay = bayPosition.IsUpper
                    //         ? machineResourcesProvider.IsDrawerInBayTop(bay.Number)
                    //         : machineResourcesProvider.IsDrawerInBayBottom(bay.Number);

                    //    if (loadingUnitOnElevator == null && bayPosition.LoadingUnit != null)
                    //    // possible pickup from bay
                    //    {
                    //        if (machineResourcesProvider.IsDrawerCompletelyOnCradle && !isDrawerInBay)
                    //        {
                    //            elevatorDataProvider.SetLoadingUnit(bayPosition.LoadingUnit.Id);
                    //            baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                    //            isChanged = true;
                    //        }
                    //    }
                    //    else if (loadingUnitOnElevator != null && bayPosition.LoadingUnit == null)
                    //    // possible deposit to bay
                    //    {
                    //        if (machineResourcesProvider.IsDrawerCompletelyOffCradle && isDrawerInBay)
                    //        {
                    //            elevatorDataProvider.SetLoadingUnit(null);
                    //            baysDataProvider.SetLoadingUnit(bayPosition.Id, loadingUnitOnElevator.Id);
                    //            loadingUnitProvider.SetHeight(loadingUnitOnElevator.Id, 0);
                    //            isChanged = true;
                    //        }
                    //    }
                    //}
                    //else if (cell != null)
                    //{
                    //    if (loadingUnitOnElevator == null && cell.LoadingUnit != null)
                    //    // possible pickup from cell
                    //    {
                    //        if (machineResourcesProvider.IsDrawerCompletelyOnCradle)
                    //        {
                    //            elevatorDataProvider.SetLoadingUnit(cell.LoadingUnit.Id);
                    //            cellsProvider.SetLoadingUnit(cell.Id, null);
                    //            isChanged = true;
                    //        }
                    //    }
                    //    else if (loadingUnitOnElevator != null && cell.LoadingUnit == null)
                    //    // possible deposit to cell
                    //    {
                    //        if (machineResourcesProvider.IsDrawerCompletelyOffCradle)
                    //        {
                    //            if (cellsProvider.CanFitLoadingUnit(cell.Id, loadingUnitOnElevator.Id))
                    //            {
                    //                elevatorDataProvider.SetLoadingUnit(null);
                    //                cellsProvider.SetLoadingUnit(cell.Id, loadingUnitOnElevator.Id);
                    //                isChanged = true;
                    //            }
                    //            else
                    //            {
                    //                this.Logger.LogWarning("Detected loading unit leaving the cradle, but cell cannot store it.");
                    //            }
                    //        }
                    //    }
                    //}

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

        #endregion
    }
}
