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
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.Direction = HorizontalMovementDirection.Backwards;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);
            int? positionId = null;
            switch (this.Mission.LoadUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.Mission.LoadUnitCellSourceId != null)
                    {
                        var cell = this.CellsProvider.GetById(this.Mission.LoadUnitCellSourceId.Value);

                        this.Mission.Direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                        && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator)
                    {
                        var bayDestination = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                        if (bayDestination != null
                            && bayDestination.Shutter.Type != ShutterType.NotSpecified
                            )
                        {
                            var shutterInverter = bayDestination.Shutter.Inverter.Index;
                            if (this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed)
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
                        positionId = bayPosition.Id;
                        if (bay.Carousel != null
                            && !bayPosition.IsUpper
                            )
                        {
                            // in lower carousel position there is no profile check barrier
                            measure = false;
                        }

                        this.Mission.Direction = (bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);
                        this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitSource);
                        var shutterInverter = bay.Shutter.Inverter.Index;
                        if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                        {
                            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                        }
#if CHECK_BAY_SENSOR
                        if (bay.Carousel != null)
                        {
                            var result = this.LoadingUnitMovementProvider.CheckBaySensors(bay, this.Mission.LoadUnitSource, deposit: false);
                            if (result != MachineErrorCode.NoError)
                            {
                                var error = this.ErrorsProvider.RecordNew(result, bay.Number);
                                throw new StateMachineException(error.Reason, bay.Number, MessageActor.MachineManager);
                            }
                        }
#endif
                    }
                    break;
            }
            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            else if (this.Mission.NeedHomingAxis == Axis.BayChain)
            {
                if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                {
                    this.Logger.LogInformation($"OpenShutter start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                }
                else
                {
                    this.Logger.LogInformation($"MoveManualLoadingUnitForward start: direction {this.Mission.Direction} Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, false, measure, this.Mission.LoadUnitId, positionId, MessageActor.MachineManager, this.Mission.TargetBay);
                }
            }
            else
            {
                this.Logger.LogInformation($"MoveLoadingUnit start: direction {this.Mission.Direction}, openShutter {this.Mission.OpenShutterPosition}, measure {measure} Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.MoveLoadingUnit(this.Mission.Direction, true, this.Mission.OpenShutterPosition, measure, MessageActor.MachineManager, this.Mission.TargetBay, this.Mission.LoadUnitId, positionId);
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
                    int? positionId = null;
                    if (notification.Type == MessageType.Homing)
                    {
                        if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                        {
                            if (!this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                            {
                                this.Mission.NeedHomingAxis = Axis.None;
                                this.MachineVolatileDataProvider.IsHomingExecuted = true;
                                this.MissionsDataProvider.Update(this.Mission);
                            }
                            // restart movement from the beginning!
                            var newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                        else if (this.Mission.NeedHomingAxis == Axis.BayChain)
                        {
                            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
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
                    else
                    {
                        if (notification.Type == MessageType.ShutterPositioning
                            || notification.TargetBay == BayNumber.ElevatorBay
                            )
                        {
                            if (this.UpdateResponseList(notification.Type))
                            {
                                this.MissionsDataProvider.Update(this.Mission);
                                this.Logger.LogTrace($"UpdateResponseList: {notification.Type} Mission:Id={this.Mission.Id}");
                                if (notification.Type == MessageType.Positioning)
                                {
                                    this.LoadUnitChangePosition();
                                }
                            }

                            if (notification.Type == MessageType.ShutterPositioning)
                            {
                                var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                                var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                                if (shutterPosition == this.Mission.OpenShutterPosition)
                                {
                                    if (this.Mission.NeedHomingAxis == Axis.BayChain)
                                    {
                                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                                        if (bay is null)
                                        {
                                            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                                            throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                                        }
                                        var bayPosition = bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitSource);
                                        positionId = bayPosition.Id;
                                        if (bay.Carousel != null
                                            && !bayPosition.IsUpper
                                            )
                                        {
                                            // in lower carousel position there is no profile check barrier
                                            measure = false;
                                        }

                                        this.Logger.LogInformation($"MoveManualLoadingUnitForward start: direction {this.Mission.Direction} Mission:Id={this.Mission.Id}");
                                        this.LoadingUnitMovementProvider.MoveManualLoadingUnitForward(this.Mission.Direction, false, measure, this.Mission.LoadUnitId, positionId, MessageActor.MachineManager, this.Mission.TargetBay);
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

                                    this.OnStop(StopRequestReason.Error, !this.ErrorsProvider.IsErrorSmall());
                                    break;
                                }
                            }
                            else if (this.Mission.NeedHomingAxis == Axis.BayChain)
                            {
                                this.Logger.LogInformation($"{this.GetType().Name}: Manual Horizontal positioning end Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.UpdateLastIdealPosition(this.Mission.Direction, false);
                            }

                            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                                && (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified
                                    || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                                )
                            {
                                this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                                if (this.Mission.NeedHomingAxis == Axis.BayChain
                                    && bay != null
                                    && bay.Positions != null
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
                    }

                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    {
                        this.OnStop(StopRequestReason.Error, !this.ErrorsProvider.IsErrorSmall());
                    }
                    break;
            }
        }

        #endregion
    }
}
