using System;
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
                this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsHomingExecuted ? Axis.None : Axis.Horizontal);
            }
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

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
                    var bay = this.LoadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                    if (bay != BayNumber.None)
                    {
                        this.Mission.CloseShutterBayNumber = bay;
                    }
                }

                if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                {
                    this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                else
                {
                    this.Logger.LogInformation($"PositionElevatorToPosition start: target {destinationHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                        this.Mission.CloseShutterBayNumber,
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
                var sourceHeight = this.LoadingUnitMovementProvider.GetSourceHeight(this.Mission, out var targetBayPositionId, out var targetCellId);

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

                if (targetCellId != null)
                {
                    var bay = this.LoadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                    if (bay != BayNumber.None)
                    {
                        this.Mission.CloseShutterBayNumber = bay;
                    }
                }
                else if (this.Mission.RestoreConditions
                    && targetBayPositionId != null)
                {
                    var bay = this.BaysDataProvider.GetByBayPositionId(targetBayPositionId.Value);
                    this.Mission.CloseShutterBayNumber = bay.Number;
                }

                if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                {
                    this.Logger.LogInformation($"Homing elevator free start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                else
                {
                    this.Logger.LogInformation($"PositionElevatorToPosition start: target {sourceHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                        this.Mission.CloseShutterBayNumber,
                        measure: false,
                        MessageActor.MachineManager,
                        this.Mission.TargetBay,
                        this.Mission.RestoreConditions,
                        targetBayPositionId,
                        targetCellId);
                }
            }
            this.Mission.Status = MissionStatus.Executing;
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

                            this.Logger.LogInformation($"PositionElevatorToPosition start: target {destinationHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}");
                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                                this.Mission.CloseShutterBayNumber,
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

                            this.Logger.LogInformation($"PositionElevatorToPosition start: target {sourceHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, measure {false}, waitContinue {false}, Mission:Id={this.Mission.Id}");
                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                                this.Mission.CloseShutterBayNumber,
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
                                var newStep = new MissionMoveLoadElevatorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep.OnEnter(null);
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        #endregion
    }
}
