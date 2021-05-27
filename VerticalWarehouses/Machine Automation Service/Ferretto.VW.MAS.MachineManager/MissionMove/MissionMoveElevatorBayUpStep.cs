using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
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
            var positionUp = bay.Positions.SingleOrDefault(s => s.IsUpper);
            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);

            if (!this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                || !this.SensorsProvider.IsLoadingUnitInLocation(positionUp.Location)
                || (this.Mission.MissionType != MissionType.WMS && this.Mission.MissionType != MissionType.OUT)
                )
            {
                var newStep = new MissionMoveBayChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);

                return true;
            }

            if (this.Mission.NeedHomingAxis == Axis.None)
            {
                var machine = this.MachineProvider.GetMinMaxHeight();
                if (Math.Abs(this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()) >= machine.HorizontalPositionToCalibrate
                    || this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Horizontal) >= machine.HorizontalCyclesToCalibrate
                    )
                {
                    this.Mission.NeedHomingAxis = Axis.Horizontal;
                }
                this.Logger.LogTrace($"NeedHomingAxis{this.Mission.NeedHomingAxis}. machine.HorizontalPositionToCalibrate {machine.HorizontalPositionToCalibrate}. machine.HorizontalCyclesToCalibrate {machine.HorizontalCyclesToCalibrate}. this.LoadingUnitMovementProvider.GetCyclesFromCalibration {this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Horizontal)}. this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition {this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()}. Mission:Id={this.Mission.Id}");

                if (Math.Abs(this.LoadingUnitMovementProvider.GetCurrentVerticalPosition()) <= 2000 &&
                    this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical) >= machine.VerticalCyclesToCalibrate &&
                   !machineResourcesProvider.IsDrawerCompletelyOnCradle
                   )
                {
                    this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                }

                this.Logger.LogTrace($"NeedHomingAxis{this.Mission.NeedHomingAxis}. machine.VerticalCyclesToCalibrate {machine.VerticalCyclesToCalibrate}. this.LoadingUnitMovementProvider.GetCyclesFromCalibration {this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical)}. this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition {this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()}. Mission:Id={this.Mission.Id}");
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

                        var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);
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
