using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
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
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator);
            var waitContinue = measure;
            this.Mission.EjectLoadUnit = false;
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.ToTarget;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (measure)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (bay is null)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                var loadUnitInBay = bay.Positions.FirstOrDefault(p => p.Location == this.Mission.LoadUnitSource)?.LoadingUnit;
                if (this.SensorsProvider.IsLoadingUnitInLocation(this.Mission.LoadUnitSource)
                    && (loadUnitInBay is null
                        || loadUnitInBay.Id == this.Mission.LoadUnitId
                        )
                    )
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotRemoved, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitNotRemoved, this.Mission.TargetBay, MessageActor.MachineManager);
                }

                if (bay.Shutter == null
                    || bay.Shutter.Type == ShutterType.NotSpecified
                    )
                {
                    this.Mission.CloseShutterBayNumber = BayNumber.None;
                }
                else
                {
                    this.Mission.CloseShutterBayNumber = bay.Number;
                }

                if (this.Mission.CloseShutterBayNumber != BayNumber.None)
                {
                    this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitSource);
                    var shutterInverter = (bay.Shutter != null) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                    if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                    {
                        this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                        this.Mission.CloseShutterBayNumber = BayNumber.None;
                    }
                }
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
            else if (measure && this.LoadingUnitMovementProvider.GetCurrentVerticalPosition() >= this.MachineProvider.GetHeight() - 500)
            {
                measure = false;
            }
            if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
            {
                if (this.Mission.CloseShutterBayNumber == BayNumber.None)
                {
                    this.Logger.LogInformation($"Homing elevator occupied start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                else
                {
                    this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.CloseShutterBayNumber, this.Mission.RestoreConditions, this.Mission.CloseShutterPosition);
                }
            }
            else
            {
                this.Logger.LogInformation($"PositionElevatorToPosition start: target {destinationHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, closeShutterPosition {this.Mission.CloseShutterPosition}, measure {measure}, waitContinue {waitContinue}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                    this.Mission.CloseShutterBayNumber,
                    this.Mission.CloseShutterPosition,
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
            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator);

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

                            this.Logger.LogInformation($"PositionElevatorToPosition start: target {destinationHeight.Value}, closeShutterBay {BayNumber.None}, closeShutterPosition {ShutterPosition.NotSpecified}, measure {measure}, waitContinue {false}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                            this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                                BayNumber.None,
                                ShutterPosition.NotSpecified,
                                measure,
                                MessageActor.MachineManager,
                                notification.RequestingBay,
                                this.Mission.RestoreConditions,
                                targetBayPositionId,
                                targetCellId);
                        }
                        else if ((notification.Type == MessageType.ShutterPositioning && this.Mission.CloseShutterBayNumber != BayNumber.None)
                            || notification.TargetBay == BayNumber.ElevatorBay
                            )
                        {
                            if (this.UpdateResponseList(notification.Type))
                            {
                                if (notification.Type == MessageType.ShutterPositioning)
                                {
                                    this.Mission.CloseShutterBayNumber = BayNumber.None;
                                    if (this.Mission.NeedHomingAxis == Axis.Horizontal || this.Mission.NeedHomingAxis == Axis.HorizontalAndVertical)
                                    {
                                        this.Logger.LogInformation($"Homing elevator occupied start Mission:Id={this.Mission.Id}");
                                        this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                                    }
                                    else
                                    {
                                        this.Logger.LogDebug($"ContinuePositioning Mission:Id={this.Mission.Id}");
                                        this.LoadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                    }
                                }
                                this.MissionsDataProvider.Update(this.Mission);
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    if (this.Mission.EjectLoadUnit
                        && (notification.Type == MessageType.ShutterPositioning
                            || notification.TargetBay == BayNumber.ElevatorBay
                            )
                        )
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
                        if (check != MachineErrorCode.NoError || this.Mission.MissionType == MissionType.ScaleCalibration)
                        {
                            this.Logger.LogDebug($"Stop movement and go back to bay. Mission:Id={this.Mission.Id}. Error:{check}");
                            this.Mission.ErrorCode = (this.Mission.MissionType == MissionType.ScaleCalibration) ? MachineErrorCode.NoError : check;
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
                IMissionMoveBase newStep = null;
                if (this.Mission.LoadUnitDestination == LoadingUnitLocation.Elevator
                    && !this.Mission.EjectLoadUnit
                    )
                {
                    newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                }
                else if (this.Mission.EjectLoadUnit)
                {
                    newStep = new MissionMoveBackToBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                }
                else if (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator)
                {
                    newStep = new MissionMoveWaitDepositCellStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                }
                else
                {
                    // Check if do not exist any waiting missions on the current bay
                    // Only reserved for internal double bay
                    if (!this.isWaitingMissionOnThisBay())
                    {
                        newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        this.Logger.LogDebug($"At least a waiting mission with step=MissionStep.WaitPick is detected and the given MissionId:{this.Mission.Id} is interrupted.");
                        newStep = new MissionMoveWaitDepositBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }

                newStep.OnEnter(null);
            }
            if ((this.MachineVolatileDataProvider.Mode != MachineMode.Manual ||
                this.MachineVolatileDataProvider.Mode != MachineMode.Manual2 ||
                this.MachineVolatileDataProvider.Mode != MachineMode.Manual3)
                && (this.Mission.CloseShutterBayNumber == BayNumber.None
                    || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                && this.isWaitingMissionOnThisBay(inError: true)
                )
            {
                var errorMission = this.MissionsDataProvider.GetAllActiveMissionsByBay(this.Mission.TargetBay)
                        .FirstOrDefault(
                            m => m.LoadUnitId != this.Mission.LoadUnitId &&
                            m.Id != this.Mission.Id &&
                            (m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick) &&
                            m.ErrorCode != MachineErrorCode.NoError);
                //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(errorMission.TargetBay);
                this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
                this.ErrorsProvider.RecordNew(errorMission.ErrorCode, this.Mission.TargetBay);
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
            }
        }

        /// <summary>
        /// Check if exist at least a waiting mission (step == MissionStep.WaitPick) in the current bay.
        /// Applied only for double bay.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if exists at least a waiting mission,
        ///     <c>false</c> otherwise.
        /// </returns>
        private bool isWaitingMissionOnThisBay(bool inError = false)
        {
            var retValue = false;

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (!(bay is null))
            {
                if (bay.IsDouble)
                {
                    // List of waiting mission on the bay
                    retValue = this.MissionsDataProvider.GetAllActiveMissionsByBay(this.Mission.TargetBay).Any(
                            m => m.LoadUnitId != this.Mission.LoadUnitId &&
                            m.Id != this.Mission.Id &&
                            (m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick) &&
                            (!inError || m.ErrorCode != MachineErrorCode.NoError)
                        );
                }

                return retValue;
            }
            else
            {
                return false;
            }
        }

        #endregion

        //private void UpdateLowerMission()
        //{
        //    var sourceBay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
        //    LoadingUnit lowerUnit = null;
        //    if (sourceBay != null
        //        && sourceBay.IsDouble
        //        && (lowerUnit = sourceBay.Positions.FirstOrDefault(p => !p.IsUpper && p.LoadingUnit != null)?.LoadingUnit) != null
        //        )
        //    {
        //        var lowerMission = this.MissionsDataProvider.GetAllActiveMissions().FirstOrDefault(m => m.LoadUnitId == lowerUnit.Id);
        //        if (lowerMission != null)
        //        {
        //            if (sourceBay.Carousel is null)
        //            {
        //                this.BaysDataProvider.AssignMission(this.Mission.TargetBay, lowerMission);
        //            }
        //            else
        //            {
        //                // carousel: wake up the bottom bay position
        //                this.Logger.LogInformation($"Resume lower bay Mission:Id={lowerMission.Id}");
        //                this.LoadingUnitMovementProvider.ResumeOperation(
        //                    lowerMission.Id,
        //                    lowerMission.LoadUnitSource,
        //                    lowerMission.LoadUnitDestination,
        //                    lowerMission.WmsId,
        //                    lowerMission.MissionType,
        //                    lowerMission.TargetBay,
        //                    MessageActor.MachineManager);
        //            }
        //        }
        //    }
        //}
    }
}
