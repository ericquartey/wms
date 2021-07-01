using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class MissionMoveLoadElevatorStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveLoadElevatorStep(Mission mission,
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
            this.Mission.Step = MissionStep.LoadElevator;
            this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);
            int? sourceBayPositionId = null;
            int? targetBayPositionId = null;
            var disableIntrusion = false;

            switch (this.Mission.LoadUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.LoadUnitCellSourceId != null)
                    {
                        var cell = this.CellsProvider.GetById(this.Mission.LoadUnitCellSourceId.Value);

                        if (this.LoadingUnitMovementProvider.IsVerticalPositionChanged(cell.Position, isEmpty: true, this.Mission.LoadUnitId))
                        {
                            this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                            this.MissionsDataProvider.Update(this.Mission);
                            this.ErrorsProvider.RecordNew(MachineErrorCode.VerticalPositionChanged, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.VerticalPositionChanged, this.Mission.TargetBay, MessageActor.MachineManager);
                        }

                        this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                        && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator)
                    {
                        var bayDestination = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                        if (bayDestination != null
                            && bayDestination.Shutter != null
                            && bayDestination.Shutter.Type != ShutterType.NotSpecified
                            )
                        {
                            var shutterInverter = bayDestination.Shutter.Inverter.Index;
                            if (this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed
                                && this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Half
                                )
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitShutterOpen, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                        }
                    }

                    break;

                default:
                    {
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                        if (bay is null)
                        {
                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                        var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitSource);
                        if (this.LoadingUnitMovementProvider.IsVerticalPositionChanged(bayPosition.Height, isEmpty: true, this.Mission.LoadUnitId))
                        {
                            this.Mission.NeedHomingAxis = Axis.HorizontalAndVertical;
                            this.MissionsDataProvider.Update(this.Mission);
                            this.ErrorsProvider.RecordNew(MachineErrorCode.VerticalPositionChanged, this.Mission.TargetBay);
                            throw new StateMachineException(ErrorDescriptions.VerticalPositionChanged, this.Mission.TargetBay, MessageActor.MachineManager);
                        }
                        sourceBayPositionId = bayPosition.Id;
                        if (bay.Carousel != null
                            && !bayPosition.IsUpper
                            )
                        {
                            // in lower carousel position there is no profile check barrier
                            measure = false;
                        }
                        else
                        {
                            this.LoadingUnitsDataProvider.SetHeight(this.Mission.LoadUnitId, 0);
                        }

                        this.Mission.Direction = (bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);
                        this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitSource);
                        var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                        if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                        {
                            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                        }
#if CHECK_BAY_SENSOR
                        var result = this.LoadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadUnitSource, deposit: false);
                        if (result != MachineErrorCode.NoError)
                        {
                            var error = this.ErrorsProvider.RecordNew(result, bay.Number);
                            throw new StateMachineException(error.Reason, bay.Number, MessageActor.MachineManager);
                        }
                        if (bay.Carousel != null
                            || bay.IsExternal
                            )
                        {
                            if (this.Mission.NeedHomingAxis == Axis.None)
                            {
                                this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);
                            }
                        }
#endif

                        if (this.MachineVolatileDataProvider.IsBayLightOn.ContainsKey(bay.Number)
                            && this.MachineVolatileDataProvider.IsBayLightOn[bay.Number]
                            && (bayPosition.IsUpper
                                || bay.Positions.FirstOrDefault(p => p.IsUpper)?.LoadingUnit is null)
                            )
                        {
                            // Light bay set to OFF. Exceptional case handling for an internal double bay
                            if (bay.IsDouble &&
                                bay.Carousel == null &&
                                !bay.IsExternal)
                            {
                                // Only for BID
                                if (bay.Positions.Any(p => p.LoadingUnit != null))
                                {
                                    // The light must be ON, because there is a loading unit into bay
                                    this.Logger.LogDebug($"Light bay {bay.Number} is {this.MachineVolatileDataProvider.IsBayLightOn[bay.Number]}");
                                }
                                else
                                {
                                    // The bay light is OFF
                                    this.BaysDataProvider.Light(this.Mission.TargetBay, false);
                                    this.Logger.LogDebug($"Light bay {bay.Number} is false");
                                }
                            }
                            else
                            {
                                // All others bay configuration
                                this.BaysDataProvider.Light(this.Mission.TargetBay, false);
                                this.Logger.LogDebug($"Light bay {bay.Number} is false");
                            }

                            if (this.BaysDataProvider.CheckIntrusion(this.Mission.TargetBay, false))
                            {
                                this.Logger.LogInformation($"Disable intrusion Mission:Id={this.Mission.Id}");
                                disableIntrusion = true;
                            }
                        }
                    }
                    break;
            }

            if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                if (bay != null)
                {
                    var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitDestination);
                    targetBayPositionId = bayPosition.Id;
                }
            }
            if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
            {
                this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            else if (this.Mission.NeedHomingAxis == Axis.BayChain
                && this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified
                )
            {
                this.Logger.LogInformation($"OpenShutter start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
            }
            else if (!disableIntrusion)
            {
                this.Logger.LogInformation($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}, measure {measure} Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, true, this.Mission.OpenShutterPosition, measure, MessageActor.MachineManager, this.Mission.TargetBay, this.Mission.LoadUnitId, this.Mission.DestinationCellId, targetBayPositionId, this.Mission.LoadUnitCellSourceId, sourceBayPositionId);
            }
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.MoveLoadingUnitStatus(notification);
            this.Logger.LogTrace($"OnNotification: type {notification.Type}, status {notification.Status}, result {notificationStatus}");

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);
                    var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                    int? targetBayPositionId = null;
                    int? sourceBayPositionId = null;
                    if (notification.Type == MessageType.Homing)
                    {
                        if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                        {
                            if (!this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                            {
                                if (this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                                {
                                    this.MachineVolatileDataProvider.IsHomingExecuted = true;
                                }
                                this.Mission.NeedHomingAxis = Axis.None;
                                this.MissionsDataProvider.Update(this.Mission);
                            }
                            // restart movement from the beginning!
                            var newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                        else if (this.Mission.NeedHomingAxis == Axis.BayChain)
                        {
                            if (bay != null
                                && bay.Positions != null
                                && bay.Positions.All(p => p.LoadingUnit is null))
                            {
                                this.Mission.NeedHomingAxis = Axis.None;
                                this.MissionsDataProvider.Update(this.Mission);
                                this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = true;
                            }
                            this.LoadUnitEnd();
                        }
                    }
                    else if (notification.Type == MessageType.ShutterPositioning
                            || notification.TargetBay == BayNumber.ElevatorBay
                            )
                    {
                        if (measure)
                        {
                            if (bay is null)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitSource);
                            sourceBayPositionId = bayPosition.Id;
                            if (bay.Carousel != null
                                && !bayPosition.IsUpper
                                )
                            {
                                // in lower carousel position there is no profile check barrier
                                measure = false;
                            }
                        }
                        if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell)
                        {
                            var bayDest = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                            if (bayDest != null)
                            {
                                var bayPosition = bayDest.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitDestination);
                                targetBayPositionId = bayPosition.Id;
                            }
                        }
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.Logger.LogTrace($"UpdateResponseList: {notification.Type} Mission:Id={this.Mission.Id}");

                            var bChangeLoadUnitPosition = false;
                            bChangeLoadUnitPosition = (notification.Type == MessageType.Positioning && !this.MachineVolatileDataProvider.IsOneTonMachine.Value) ||
                                                         (notification.Type == MessageType.CombinedMovements && this.MachineVolatileDataProvider.IsOneTonMachine.Value);
                            if (bChangeLoadUnitPosition)
                            {
                                this.LoadUnitChangePosition();
                            }
                            else if (notification.Type == MessageType.CheckIntrusion)
                            {
                                this.Logger.LogInformation($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}, measure {measure} Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, true, this.Mission.OpenShutterPosition, measure, MessageActor.MachineManager, this.Mission.TargetBay, this.Mission.LoadUnitId, this.Mission.DestinationCellId, targetBayPositionId, this.Mission.LoadUnitCellSourceId, sourceBayPositionId);
                            }
                            this.MissionsDataProvider.Update(this.Mission);
                        }

                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition == this.Mission.OpenShutterPosition)
                            {
                                if (this.Mission.NeedHomingAxis == Axis.BayChain)
                                {
                                    this.Logger.LogInformation($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {ShutterPosition.NotSpecified}, measure {measure} Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, true, ShutterPosition.NotSpecified, measure, MessageActor.MachineManager, this.Mission.TargetBay, this.Mission.LoadUnitId, this.Mission.DestinationCellId, targetBayPositionId, this.Mission.LoadUnitCellSourceId, sourceBayPositionId);
                                }
                                else
                                {
                                    this.Logger.LogDebug($"ContinuePositioning Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                }
                            }
                            else
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error, moveBackward: true);
                                break;
                            }
                        }

                        var isMovementEnded = (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning) && !(this.MachineVolatileDataProvider.IsOneTonMachine.Value)) ||
                                     (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.CombinedMovements) && (this.MachineVolatileDataProvider.IsOneTonMachine.Value));

                        if (isMovementEnded
                            && (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified
                                || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                            )
                        {
                            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                            bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                            if (this.Mission.NeedHomingAxis == Axis.None
                                && bay != null
                                && bay.Positions != null
                                && (bay.Carousel != null || bay.IsExternal)
                                && bay.TotalCycles - bay.LastCalibrationCycles >= bay.CyclesToCalibrate)
                            {
                                this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;
                                this.Mission.NeedHomingAxis = Axis.BayChain;
                                this.Logger.LogTrace($"NeedHomingAxis{this.Mission.NeedHomingAxis}. CyclesToCalibrate {bay.CyclesToCalibrate}. LastCalibrationCycles {bay.LastCalibrationCycles}. Total cycles {bay.TotalCycles}. Mission:Id={this.Mission.Id}");
                            }

                            if (this.Mission.NeedHomingAxis == Axis.BayChain
                                && bay != null
                                && bay.Positions != null
                                && (bay.Carousel != null || bay.IsExternal)
                                && bay.Positions.All(p => p.LoadingUnit is null)
                                )
                            {
                                this.MissionsDataProvider.Update(this.Mission);
                                this.Logger.LogInformation($"Homing Bay free start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadUnitId, true, notification.RequestingBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.LoadUnitEnd();
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
