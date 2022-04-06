using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveElevatorBayUpStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveElevatorBayUpStep(Mission mission,
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
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.ElevatorBayUp;
            this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            //this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            //this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var machineResourcesProvider = this.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();

            var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);

            if (bay.IsExternal &&
                bay.IsDouble)
            {
                if (machineResourcesProvider.IsDrawerInBayInternalPosition(this.Mission.TargetBay, true) != machineResourcesProvider.IsDrawerInBayExternalPosition(this.Mission.TargetBay, true)
                    || (this.Mission.MissionType != MissionType.WMS && this.Mission.MissionType != MissionType.OUT && this.Mission.MissionType != MissionType.FullTestOUT))
                {
                    var newStep = new MissionMoveWaitDepositExternalBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);

                    return true;
                }

                if (this.Mission.NeedHomingAxis == Axis.None)
                {
                    var machine = this.MachineProvider.GetMinMaxHeight();

                    if (this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical) >= machine.VerticalCyclesToCalibrate
                        && !machineResourcesProvider.IsDrawerCompletelyOnCradle
                        && (bay.Shutter == null || bay.Shutter.Type == ShutterType.NotSpecified)
                       )
                    {
                        this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                        this.Logger.LogDebug($"Generate Homing. Vertical cycles {this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical)} expired {machine.VerticalCyclesToCalibrate}, Mission:Id={this.Mission.Id}");
                    }
                    if (this.Mission.NeedHomingAxis != Axis.HorizontalAndVertical
                        && (Math.Abs(this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()) >= machine.HorizontalPositionToCalibrate
                            || this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Horizontal) >= machine.HorizontalCyclesToCalibrate
                            )
                        )
                    {
                        this.Mission.NeedHomingAxis = Axis.Horizontal;
                        this.Logger.LogDebug($"Generate Homing. Horizontal cycles {this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Horizontal)} expired {machine.HorizontalCyclesToCalibrate} or position {Math.Abs(this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()):0.00}, Mission:Id={this.Mission.Id}");
                    }
                }
                if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                {
                    if (this.Mission.CloseShutterBayNumber == BayNumber.None)
                    {
                        this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, false, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    else
                    {
                        this.Logger.LogInformation($"{this.GetType().Name}: Shutter Close start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                    }
                }
                else
                {
                    var position = bay.Positions.SingleOrDefault(s => s.Location == this.Mission.LoadUnitDestination);
                    var destination = bay.Positions.SingleOrDefault(s => s.IsUpper != position.IsUpper);

                    this.LoadingUnitMovementProvider.PositionElevatorToPosition(destination.Height,
                                        BayNumber.None,
                                        this.Mission.CloseShutterPosition,
                                        measure: false,
                                        MessageActor.MachineManager,
                                        this.Mission.TargetBay,
                                        this.Mission.RestoreConditions,
                                        this.Mission.LoadUnitId,
                                        destination.Id,
                                        null);
                }

                this.Mission.Status = MissionStatus.Executing;
                this.MissionsDataProvider.Update(this.Mission);
            }
            else
            {
                var positionUp = bay.Positions.SingleOrDefault(s => s.IsUpper);
                var destination = bay.Positions.SingleOrDefault(s => !s.IsUpper);

                if (!this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                    || !this.SensorsProvider.IsLoadingUnitInLocation(positionUp.Location)
                    || (this.Mission.MissionType != MissionType.WMS && this.Mission.MissionType != MissionType.OUT && this.Mission.MissionType != MissionType.FullTestOUT)
                    )
                {
                    var newStep = new MissionMoveBayChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);

                    return true;
                }

                if (this.Mission.NeedHomingAxis == Axis.None)
                {
                    var machine = this.MachineProvider.GetMinMaxHeight();
                    if (this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical) >= machine.VerticalCyclesToCalibrate
                        && !machineResourcesProvider.IsDrawerCompletelyOnCradle
                       )
                    {
                        this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                        this.Logger.LogDebug($"Generate Homing. Vertical cycles {this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical)} expired {machine.VerticalCyclesToCalibrate}, Mission:Id={this.Mission.Id}");
                    }

                    if (this.Mission.NeedHomingAxis != Axis.HorizontalAndVertical
                        && (
                            Math.Abs(this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()) >= machine.HorizontalPositionToCalibrate
                            || this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Horizontal) >= machine.HorizontalCyclesToCalibrate
                            )
                        )
                    {
                        this.Mission.NeedHomingAxis = Axis.Horizontal;
                        this.Logger.LogDebug($"Generate Homing. Horizontal cycles {this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Horizontal)} expired {machine.HorizontalCyclesToCalibrate} or position {Math.Abs(this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()):0.00}, Mission:Id={this.Mission.Id}");
                    }
                }
                if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                {
                    if (this.Mission.CloseShutterBayNumber == BayNumber.None)
                    {
                        this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, false, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                    else
                    {
                        this.Logger.LogInformation($"{this.GetType().Name}: Shutter Close start Mission:Id={this.Mission.Id}");
                        this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                    }
                }
                else
                {
                    this.LoadingUnitMovementProvider.PositionElevatorToPosition(positionUp.Height,
                                        BayNumber.None,
                                        this.Mission.CloseShutterPosition,
                                        measure: false,
                                        MessageActor.MachineManager,
                                        this.Mission.TargetBay,
                                        this.Mission.RestoreConditions,
                                        this.Mission.LoadUnitId,
                                        positionUp.Id,
                                        null);
                }

                this.Mission.Status = MissionStatus.Executing;
                this.MissionsDataProvider.Update(this.Mission);
            }

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);

                    if (bay.IsExternal &&
                        bay.IsDouble)
                    {
                        if (notification.Type == MessageType.Homing)
                        {
                            this.Mission.NeedHomingAxis = Axis.None;
                            this.MissionsDataProvider.Update(this.Mission);

                            this.MachineVolatileDataProvider.IsHomingExecuted = true;

                            var position = bay.Positions.SingleOrDefault(s => s.Location == this.Mission.LoadUnitDestination);
                            var destination = bay.Positions.SingleOrDefault(s => s.IsUpper != position.IsUpper);

                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(destination.Height,
                                                BayNumber.None,
                                                this.Mission.CloseShutterPosition,
                                                measure: false,
                                                MessageActor.MachineManager,
                                                this.Mission.TargetBay,
                                                this.Mission.RestoreConditions,
                                                this.Mission.LoadUnitId,
                                                destination.Id,
                                                null);
                        }
                        else
                        {
                            var newStep = new MissionMoveWaitDepositExternalBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                    }
                    else
                    {
                        if (notification.Type == MessageType.Homing)
                        {
                            this.Mission.NeedHomingAxis = Axis.None;
                            this.MissionsDataProvider.Update(this.Mission);

                            this.MachineVolatileDataProvider.IsHomingExecuted = true;
                            var positionUp = bay.Positions.SingleOrDefault(s => s.IsUpper);

                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(positionUp.Height,
                                 BayNumber.None,
                                 this.Mission.CloseShutterPosition,
                                 measure: false,
                                 MessageActor.MachineManager,
                                 this.Mission.TargetBay,
                                 this.Mission.RestoreConditions,
                                 this.Mission.LoadUnitId,
                                 positionUp.Id,
                                 null);
                        }
                        else
                        {
                            var newStep = new MissionMoveBayChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        #endregion
    }
}
