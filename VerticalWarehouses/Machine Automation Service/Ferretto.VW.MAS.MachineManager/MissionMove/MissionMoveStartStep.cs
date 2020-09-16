using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveStartStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveStartStep(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.Start;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.ErrorCode = MachineErrorCode.NoError;
            if (this.Mission.NeedHomingAxis == Axis.None)
            {
                this.MachineVolatileDataProvider.IsHomingExecuted = this.MachineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay];
                this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsHomingExecuted ? Axis.None : Axis.HorizontalAndVertical);
            }
            this.Mission.Status = MissionStatus.Executing;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var disableIntrusion = false;

            if (this.Mission.LoadUnitSource is LoadingUnitLocation.Elevator)
            {
                var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
                if (destinationHeight is null)
                {
                    if (this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    else
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                }
                if (targetCellId != null)
                {
                    var bayNumber = this.LoadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                    if (bayNumber != BayNumber.None)
                    {
                        var bay = this.BaysDataProvider.GetByNumber(bayNumber);
                        if (bay.Shutter != null &&
                            bay.Shutter.Type != ShutterType.NotSpecified)
                        {
                            var shutterInverter = (bay.Shutter != null) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition != ShutterPosition.Closed
                                 && shutterPosition != ShutterPosition.Half
                                )
                            {
                                if (bayNumber != this.Mission.TargetBay)
                                {
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, bayNumber);
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitShutterOpen, bayNumber, MessageActor.MachineManager);
                                }
                                this.Mission.CloseShutterBayNumber = bayNumber;
                                this.Mission.CloseShutterPosition = ShutterPosition.Closed;
                            }
                        }
                    }
                }

                if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                {
                    if (this.Mission.CloseShutterBayNumber == BayNumber.None)
                    {
                        this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    else
                    {
                        this.Logger.LogInformation($"{this.GetType().Name}: Shutter Close start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                    }
                }
                else
                {
                    this.Logger.LogInformation($"PositionElevatorToPosition start: target {destinationHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, closeShutterPosition {this.Mission.CloseShutterPosition}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                    this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                        this.Mission.CloseShutterBayNumber,
                        this.Mission.CloseShutterPosition,
                        measure: false,
                        MessageActor.MachineManager,
                        this.Mission.TargetBay,
                        this.Mission.RestoreConditions,
                        targetBayPositionId,
                        targetCellId);
                }
            }
            else
            {
                var sourceHeight = this.LoadingUnitMovementProvider.GetSourceHeight(this.Mission, out var sourceBayPositionId, out var sourceCellId);

                if (sourceHeight is null)
                {
                    if (this.Mission.LoadUnitSource == LoadingUnitLocation.Cell || this.Mission.LoadUnitSource == LoadingUnitLocation.LoadUnit)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitSourceCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    else
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                }

                if (sourceCellId != null)
                {
                    var bayNumber = this.LoadingUnitMovementProvider.GetBayByCell(sourceCellId.Value);
                    if (bayNumber != BayNumber.None)
                    {
                        var bay = this.BaysDataProvider.GetByNumber(bayNumber);
                        if (bay.Shutter != null &&
                            bay.Shutter.Type != ShutterType.NotSpecified)
                        {
                            var shutterInverter = (bay.Shutter != null) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition != ShutterPosition.Closed
                                 && shutterPosition != ShutterPosition.Half
                                )
                            {
                                if (bayNumber != this.Mission.TargetBay)
                                {
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, bayNumber);
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitShutterOpen, bayNumber, MessageActor.MachineManager);
                                }
                                this.Mission.CloseShutterBayNumber = bayNumber;
                                this.Mission.CloseShutterPosition = ShutterPosition.Closed;
                            }
                        }
                    }
                }
                else if (sourceBayPositionId != null)
                {
                    var bay = this.BaysDataProvider.GetByBayPositionId(sourceBayPositionId.Value);
                    var bayPosition = bay.Positions.First(p => p.Id == sourceBayPositionId.Value);

                    if (this.MachineVolatileDataProvider.IsBayLightOn.ContainsKey(bay.Number)
                        && this.MachineVolatileDataProvider.IsBayLightOn[bay.Number]
                        && (bayPosition.IsUpper
                            || bay.Positions.FirstOrDefault(p => p.IsUpper)?.LoadingUnit is null)
                        )
                    {
                        this.BaysDataProvider.Light(this.Mission.TargetBay, false);
                        if (this.BaysDataProvider.CheckIntrusion(this.Mission.TargetBay, false))
                        {
                            this.Logger.LogInformation($"Disable intrusion Mission:Id={this.Mission.Id}");
                            disableIntrusion = true;
                        }
                    }

                    if (this.Mission.RestoreConditions
                        && bay.Shutter != null
                        && bay.Shutter.Type != ShutterType.NotSpecified
                        )
                    {
                        this.Mission.CloseShutterBayNumber = bay.Number;
                        this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, bayPosition.Location);
                    }

                    this.LoadingUnitsDataProvider.SetStatus(this.Mission.LoadUnitId, DataModels.Enumerations.LoadingUnitStatus.OnMovementToLocation);
                    //this.NotifyAssignedMissionChanged(bay.Number, null);
                }
                if (this.Mission.NeedHomingAxis == Axis.None)
                {
                    var machine = this.MachineProvider.Get();
                    if (Math.Abs(this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()) >= machine.HorizontalPositionToCalibrate
                        || this.LoadingUnitMovementProvider.GetCyclesFromCalibration() >= machine.HorizontalCyclesToCalibrate
                        )
                    {
                        this.Mission.NeedHomingAxis = Axis.Horizontal;
                    }
                    this.Logger.LogTrace($"NeedHomingAxis{this.Mission.NeedHomingAxis}. machine.HorizontalPositionToCalibrate {machine.HorizontalPositionToCalibrate}. machine.HorizontalCyclesToCalibrate {machine.HorizontalCyclesToCalibrate}. this.LoadingUnitMovementProvider.GetCyclesFromCalibration {this.LoadingUnitMovementProvider.GetCyclesFromCalibration()}. this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition {this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()}. Mission:Id={this.Mission.Id}");
                }

                if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                {
                    if (this.Mission.CloseShutterBayNumber == BayNumber.None)
                    {
                        this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    else
                    {
                        this.Logger.LogInformation($"{this.GetType().Name}: Shutter Close start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                    }
                }
                else if (!disableIntrusion)
                {
                    this.Logger.LogInformation($"PositionElevatorToPosition start: target {sourceHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, closeShutterPosition {this.Mission.CloseShutterPosition}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                    this.LoadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                        this.Mission.CloseShutterBayNumber,
                        this.Mission.CloseShutterPosition,
                        measure: false,
                        MessageActor.MachineManager,
                        this.Mission.TargetBay,
                        this.Mission.RestoreConditions,
                        sourceBayPositionId,
                        sourceCellId);
                }
            }
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationStart);

            if (this.Mission.MissionType == MissionType.LoadUnitOperation)
            {
                this.MachineVolatileDataProvider.Mode = MachineMode.LoadUnitOperations;
                this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
            }
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.Homing)
                    {
                        this.Mission.NeedHomingAxis = Axis.None;
                        this.MissionsDataProvider.Update(this.Mission);

                        this.MachineVolatileDataProvider.IsHomingExecuted = true;

                        if (this.Mission.LoadUnitSource is LoadingUnitLocation.Elevator)
                        {
                            var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);

                            this.Logger.LogInformation($"PositionElevatorToPosition start: target {destinationHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, closeShutterPosition {this.Mission.CloseShutterPosition}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                                this.Mission.CloseShutterBayNumber,
                                this.Mission.CloseShutterPosition,
                                measure: false,
                                MessageActor.MachineManager,
                                this.Mission.TargetBay,
                                this.Mission.RestoreConditions,
                                targetBayPositionId,
                                targetCellId);
                        }
                        else
                        {
                            var sourceHeight = this.LoadingUnitMovementProvider.GetSourceHeight(this.Mission, out var targetBayPositionId, out var targetCellId);

                            this.Logger.LogInformation($"PositionElevatorToPosition start: target {sourceHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, closeShutterPosition {this.Mission.CloseShutterPosition}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                                this.Mission.CloseShutterBayNumber,
                                this.Mission.CloseShutterPosition,
                                measure: false,
                                MessageActor.MachineManager,
                                this.Mission.TargetBay,
                                this.Mission.RestoreConditions,
                                targetBayPositionId,
                                targetCellId);
                        }
                    }
                    else
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            if (notification.Type == MessageType.ShutterPositioning
                                && (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                                )
                            {
                                this.Mission.CloseShutterBayNumber = BayNumber.None;
                                this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            else if (notification.Type == MessageType.CheckIntrusion && this.Mission.NeedHomingAxis == Axis.None)
                            {
                                var sourceHeight = this.LoadingUnitMovementProvider.GetSourceHeight(this.Mission, out var sourceBayPositionId, out var sourceCellId);

                                this.Logger.LogInformation($"PositionElevatorToPosition start: target {sourceHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, closeShutterPosition {this.Mission.CloseShutterPosition}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                                this.LoadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                                    this.Mission.CloseShutterBayNumber,
                                    this.Mission.CloseShutterPosition,
                                    measure: false,
                                    MessageActor.MachineManager,
                                    this.Mission.TargetBay,
                                    this.Mission.RestoreConditions,
                                    sourceBayPositionId,
                                    sourceCellId);
                            }
                            this.MissionsDataProvider.Update(this.Mission);
                        }

                        if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                            && (this.Mission.CloseShutterBayNumber == BayNumber.None
                                || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                            )
                        {
                            if (this.Mission.LoadUnitSource is LoadingUnitLocation.Elevator)
                            {
                                var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep.OnEnter(null);
                            }
                            else
                            {
                                // Retrieve the bay related to the source location (if exists)
                                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);

                                if (bay != null)
                                {
                                    if (bay.IsExternal)
                                    {
                                        // Handle the external bay with a proper step
                                        var isExternalBayMovementRequested = bay.IsExternal &&
                                        this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number) &&
                                        !this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number);

                                        if ((this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number) &&
                                            this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)) ||
                                            (!this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number) &&
                                            !this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)))
                                        {
                                            var newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                            newStep.OnEnter(null);
                                            break;
                                        }

                                        if (isExternalBayMovementRequested)
                                        {
                                            // Move the external bay
                                            var newStep = new MissionMoveExtBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                            newStep.OnEnter(null);
                                        }
                                    }
                                    else
                                    {
                                        var newStep = new MissionMoveLoadElevatorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                        newStep.OnEnter(null);
                                    }
                                }
                                else
                                {
                                    var newStep = new MissionMoveLoadElevatorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                    newStep.OnEnter(null);
                                }
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        #endregion
    }
}
