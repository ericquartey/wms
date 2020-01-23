using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    /// <summary>
    /// this step moves vertically elevator with Load Unit on board to destination.
    ///     if source is Bay it closes shutter (before moving) and scales weight
    ///     if homing is needed it closes shutter, then does homing, and then moves
    ///     if source is bay and weight is wrong it comes back to bay
    /// </summary>
    public class MissionMoveToTargetStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveToTargetStep(Mission mission,
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
            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);
            var waitContinue = measure;
            this.Mission.EjectLoadUnit = false;
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.ToTarget;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (bay is null)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                this.Mission.CloseShutterBayNumber = (bay.Shutter.Type != ShutterType.NotSpecified ? bay.Number : BayNumber.None);
                measure = true;
                waitContinue = (this.Mission.CloseShutterBayNumber != BayNumber.None && !bay.IsExternal);
            }

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
            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                if (this.Mission.CloseShutterBayNumber == BayNumber.None)
                {
                    this.Logger.LogDebug($"Homing elevator occupied start");
                    this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                else
                {
                    this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.CloseShutterBayNumber, this.Mission.RestoreConditions);
                }
            }
            else
            {
                this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                    this.Mission.CloseShutterBayNumber,
                    measure,
                    MessageActor.MachineManager,
                    this.Mission.TargetBay,
                    this.Mission.RestoreConditions,
                    targetBayPositionId,
                    targetCellId,
                    waitContinue);
                this.Mission.RestoreConditions = false;
            }
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);
            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.RequestingBay == this.Mission.TargetBay)
                    {
                        if (notification.Type == MessageType.Homing
                            && notification.Data is HomingMessageData messageData
                            && messageData.AxisToCalibrate != Axis.BayChain
                            )
                        {
                            if (!this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                            {
                                this.Mission.NeedHomingAxis = Axis.None;
                                this.MissionsDataProvider.Update(this.Mission);
                            }

                            var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);

                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                                BayNumber.None,
                                measure,
                                MessageActor.MachineManager,
                                notification.RequestingBay,
                                this.Mission.RestoreConditions,
                                targetBayPositionId,
                                targetCellId);
                        }
                        else if (notification.Type == MessageType.ShutterPositioning
                            || notification.TargetBay == BayNumber.ElevatorBay
                            )
                        {
                            if (this.UpdateResponseList(notification.Type))
                            {
                                this.MissionsDataProvider.Update(this.Mission);
                                if (notification.Type == MessageType.ShutterPositioning)
                                {
                                    if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                                    {
                                        this.Logger.LogDebug($"Homing elevator occupied start");
                                        this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                                    }
                                    else
                                    {
                                        this.Logger.LogDebug($"ContinuePositioning");
                                        this.LoadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    if (this.Mission.EjectLoadUnit)
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                        }
                    }
                    else
                    {
                        this.OnStop(StopRequestReason.Error);
                        return;
                    }
                    break;

                case MessageStatus.OperationUpdateData:
                    // check weight value
                    if (measure
                        && !this.Mission.EjectLoadUnit
                        && notification.Source != MessageActor.MachineManager
                        )
                    {
                        var check = this.LoadingUnitsDataProvider.CheckWeight(this.Mission.LoadUnitId);
                        //check = MachineErrorCode.LoadUnitWeightExceeded;    // TEST
                        if (check == MachineErrorCode.NoError)
                        {
                            this.BaysDataProvider.Light(this.Mission.TargetBay, false);

                            // wake up the bottom bay position
                            var activeMission = this.MissionsDataProvider.GetAllActiveMissionsByBay(this.Mission.TargetBay)
                                .FirstOrDefault(x => x.Status == MissionStatus.Waiting);
                            if (activeMission != null)
                            {
                                this.LoadingUnitMovementProvider.ResumeOperation(activeMission.Id, activeMission.LoadUnitSource, activeMission.LoadUnitDestination, activeMission.WmsId, activeMission.TargetBay, MessageActor.MachineManager);
                            }
                        }
                        else
                        {
                            // stop movement and go back to bay
                            this.ErrorsProvider.RecordNew(check, this.Mission.TargetBay);
                            this.Mission.EjectLoadUnit = true;
                            this.Mission.LoadUnitDestination = this.Mission.LoadUnitSource;
                            this.Mission.RestoreConditions = true;
                            this.MissionsDataProvider.Update(this.Mission);

                            var newMessageData = new StopMessageData(StopRequestReason.Stop);
                            this.LoadingUnitMovementProvider.StopOperation(newMessageData, notification.RequestingBay, MessageActor.MachineManager, notification.RequestingBay);
                        }
                    }

                    break;
            }

            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                && (this.Mission.CloseShutterBayNumber == BayNumber.None
                    || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                )
            {
                if (this.Mission.LoadUnitDestination == LoadingUnitLocation.Elevator
                    && !this.Mission.EjectLoadUnit
                    )
                {
                    var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else if (this.Mission.EjectLoadUnit)
                {
                    var newStep = new MissionMoveBackToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
            }
        }

        #endregion
    }
}
