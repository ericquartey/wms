using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveDepositUnitStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveDepositUnitStep(Mission mission,
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
            this.Mission.Step = MissionStep.DepositUnit;
            this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            var bayNumber = this.Mission.TargetBay;
            var fastDeposit = true;
            int? targetBayPositionId = null;
            int? sourceBayPositionId = null;
            switch (this.Mission.LoadUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.DestinationCellId is null)
                    {
                        this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationCell, this.Mission.TargetBay, MessageActor.MachineManager);
                    }

                    if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell
                        && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator)
                    {
                        var baySource = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                        if (baySource != null
                            && baySource.Shutter != null
                            && baySource.Shutter.Type != ShutterType.NotSpecified
                            )
                        {
                            var shutterInverter = baySource.Shutter.Inverter.Index;
                            if (shutterInverter != InverterDriver.Contracts.InverterIndex.None
                                && this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed
                                && this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Half
                                )
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitShutterOpen, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                        }
                    }

                    var cell = this.CellsProvider.GetById(this.Mission.DestinationCellId.Value);
                    if (this.LoadingUnitMovementProvider.IsVerticalPositionChanged(cell.Position, isEmpty: false, this.Mission.LoadUnitId))
                    {
                        this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                        this.MissionsDataProvider.Update(this.Mission);
                        this.ErrorsProvider.RecordNew(MachineErrorCode.VerticalPositionChanged, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.VerticalPositionChanged, this.Mission.TargetBay, MessageActor.MachineManager);
                    }

                    this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;

                    break;

                default:
                    {
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                        if (bay is null)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                        this.Mission.Direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        bayNumber = bay.Number;
                        var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitDestination);
                        if (this.LoadingUnitMovementProvider.IsVerticalPositionChanged(bayPosition.Height, isEmpty: false, this.Mission.LoadUnitId))
                        {
                            this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                            this.MissionsDataProvider.Update(this.Mission);
                            this.ErrorsProvider.RecordNew(MachineErrorCode.VerticalPositionChanged, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.VerticalPositionChanged, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                        targetBayPositionId = bayPosition.Id;
                        this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitDestination);
                        var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(bay.Number);
                        if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                        {
                            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                        }
#if CHECK_BAY_SENSOR
                        if (bay.Carousel != null ||
                            bay.External != null)
                        {
                            var result = this.LoadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadUnitDestination, deposit: true);
                            if (result != MachineErrorCode.NoError)
                            {
                                var error = this.ErrorsProvider.RecordNew(result, bayNumber);
                                throw new StateMachineException(error.Reason, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                        }
                        if (!bay.IsFastDepositToBay
                            && (bay.Carousel == null
                                || bay.Positions.Any(p => p.Location == this.Mission.LoadUnitDestination)
                                )
                            )
                        {
                            fastDeposit = false;
                        }
#endif
                        if (bay.Number != this.Mission.TargetBay)
                        {
                            this.Logger.LogInformation($"Bay number changed to {bay.Number} for Mission:Id={this.Mission.Id}");
                            this.Mission.TargetBay = bay.Number;
                        }
                    }
                    break;
            }

            if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (bay != null)
                {
                    var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitSource);
                    sourceBayPositionId = bayPosition.Id;
                }
            }

            if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
            {
                this.Mission.ErrorMovements |= MissionErrorMovements.MoveForward;
                if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                {
                    this.Logger.LogInformation($"Open Shutter Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, this.Mission.RestoreConditions);
                }
                else
                {
                    this.Logger.LogInformation($"Manual Horizontal forward positioning start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, null, MessageActor.MachineManager, this.Mission.TargetBay);
                }
            }
            else
            {
                this.ElevatorDataProvider.UpdateLastIdealPosition(this.LoadingUnitMovementProvider.GetCurrentVerticalPosition(), Orientation.Vertical);

                this.Logger.LogInformation($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition} Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction,
                    moveToCradle: false,
                    this.Mission.OpenShutterPosition,
                    measure: false,
                    MessageActor.MachineManager,
                    bayNumber,
                    this.Mission.LoadUnitId,
                    this.Mission.DestinationCellId, targetBayPositionId,
                    this.Mission.LoadUnitCellSourceId,
                    sourceBayPositionId,
                    fastDeposit);

                var machine = this.MachineProvider.GetMinMaxHeight();
                if (this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell &&
                    this.Mission.LoadUnitSource != LoadingUnitLocation.Cell &&
                    this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator &&
                    this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical) >= machine.VerticalCyclesToCalibrate
                   )
                {
                    var baySource = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);
                    if (!baySource.IsDouble
                        || baySource.Positions.Any(p => p.IsBlocked)
                        || this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical) >= (machine.VerticalCyclesToCalibrate * 1.5)
                        )
                    {
                        this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                        this.Logger.LogDebug($"Generate Homing. Vertical cycles {this.LoadingUnitMovementProvider.GetCyclesFromCalibration(Orientation.Vertical)} expired {machine.VerticalCyclesToCalibrate}, Mission:Id={this.Mission.Id}");
                    }
                }
            }
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.MoveLoadingUnitStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.ShutterPositioning
                        || notification.TargetBay == BayNumber.ElevatorBay
                        )
                    {
                        if (notification.Type == MessageType.Homing
                            && (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                            )
                        {
                            this.Mission.NeedHomingAxis = Axis.None;
                            if (this.Mission.DeviceNotifications == MissionDeviceNotifications.None)
                            {
                                this.DepositUnitEnd();
                                break;
                            }
                        }

                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);

                            var bChangeDepositUnitPosition = false;
                            bChangeDepositUnitPosition = (notification.Type == MessageType.Positioning && !this.MachineVolatileDataProvider.IsOneTonMachine.Value) ||
                                                     (notification.Type == MessageType.CombinedMovements && this.MachineVolatileDataProvider.IsOneTonMachine.Value);
                            if (bChangeDepositUnitPosition)
                            {
                                this.DepositUnitChangePosition();
                            }
                        }

                        if (notification.Type == MessageType.ShutterPositioning
                            && this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified
                            )
                        {
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition == this.Mission.OpenShutterPosition)
                            {
                                // Light OFF, if a loading unit is waiting into bay for a internal double bay
                                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);

                                if (this.MachineVolatileDataProvider.IsBayLightOn.ContainsKey(this.Mission.TargetBay) &&
                                    bay.IsDouble &&
                                    bay.Carousel == null &&
                                    !bay.IsExternal)
                                {
                                    // Handle only for BID
                                    var waitMissions = this.MissionsDataProvider.GetAllMissions()
                                        .Where(
                                            m => m.LoadUnitId != this.Mission.LoadUnitId &&
                                            m.Id != this.Mission.Id &&
                                            m.Status == MissionStatus.Waiting &&
                                            m.Step == MissionStep.WaitPick &&
                                            bay.Positions.Any(p => p.LoadingUnit?.Id == m.LoadUnitId)
                                        );

                                    if (waitMissions.Any())
                                    {
                                        if (this.MachineVolatileDataProvider.IsBayLightOn[this.Mission.TargetBay])
                                        {
                                            this.BaysDataProvider.Light(this.Mission.TargetBay, false);
                                        }
                                    }
                                }

                                if (this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveForward)
                                    && (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                                    )
                                {
                                    this.Logger.LogInformation($"Manual Horizontal forward positioning start Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, true, false, this.Mission.LoadUnitId, null, MessageActor.MachineManager, this.Mission.TargetBay);
                                }
                                else
                                {
                                    this.Logger.LogDebug($"ContinuePositioning Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                }
                            }
                            else
                            {
                                this.Logger.LogError(ErrorDescriptions.LoadUnitShutterClosed);
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error, moveBackward: true);
                                break;
                            }
                        }
                        else if (this.Mission.ErrorMovements.HasFlag(MissionErrorMovements.MoveForward))
                        {
                            this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal positioning end Mission:Id={this.Mission.Id}");
                            this.LoadingUnitMovementProvider.UpdateLastIdealPosition(this.Mission.Direction, true);
                            this.Mission.ErrorMovements = MissionErrorMovements.None;
                            this.MissionsDataProvider.Update(this.Mission);
                        }

                        var isMovementEnded = (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning) && !(this.MachineVolatileDataProvider.IsOneTonMachine.Value)) ||
                                         (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.CombinedMovements) && (this.MachineVolatileDataProvider.IsOneTonMachine.Value));

                        if (isMovementEnded
                            && (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified
                                || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                            )
                        {
                            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                            if (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified
                                && (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                                )
                            {
                                this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.DepositUnitEnd();
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    {
                        this.OnStop(StopRequestReason.Error, moveBackward: true);
                    }
                    break;
            }
        }

        #endregion
    }
}
