using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveExtBayStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveExtBayStep(Mission mission,
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
            this.Mission.Step = MissionStep.ExtBay;
            this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            //this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }

            var destination = bay.Positions.FirstOrDefault();

            // Detect if homing operation is required
            //this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);

            if (this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == destination.Location && m.Id != this.Mission.Id))
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, bay.Number, MessageActor.MachineManager);
            }

            var isLoadUnitDestinationInBay = (destination.Location == this.Mission.LoadUnitDestination);

            //if (this.Mission.NeedHomingAxis == Axis.BayChain)
            //{
            //    this.Logger.LogInformation($"Homing External Bay Start Mission:Id={this.Mission.Id}");
            //    this.LoadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadUnitId, true, bay.Number, MessageActor.MachineManager);
            //}
            //else if (this.Mission.RestoreConditions)
            //{
            //    this.Logger.LogDebug($"Move in restore conditions => LoadUnitDestination: {this.Mission.LoadUnitDestination}, bay number: {bay.Number}");
            //    if (!this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId,
            //        (isLoadUnitDestinationInBay ? ExternalBayMovementDirection.TowardOperator : ExternalBayMovementDirection.TowardMachine),
            //        MessageActor.MachineManager,
            //        bay.Number,
            //        restore: true))
            //    {
            //        // already arrived
            //        this.ExternalBayChainEnd();
            //        return true;
            //    }
            //}
            if (this.Mission.RestoreConditions)
            {
                this.Mission.RestoreConditions = false;
                this.Mission.ErrorCode = MachineErrorCode.NoError;
                this.Logger.LogInformation($"Homing External Bay Start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadUnitId, true, bay.Number, MessageActor.MachineManager);
            }
            else
            {
                this.Logger.LogDebug($"Move into external bay => LoadUnitDestination: {this.Mission.LoadUnitDestination}, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");

                // Move during normal positioning
                if (isLoadUnitDestinationInBay)
                {
                    if (this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                    {
                        if (!this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions))
                        {
                            this.ExternalBayChainEnd();
                            return true;
                        }

                        this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                        var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                        if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                        {
                            this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                        }
                        if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified)
                        {
                            this.Logger.LogInformation($"CloseShutter start Mission:Id={this.Mission.Id}");
                            this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                        }
                    }
                    else
                    {
                        this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
                    }
                    //if (this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number))
                    //{
                    //    // No movement for external bay is required
                    //}
                }
                else
                {
                    if (this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number))
                    {
                        if (!this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions))
                        {
                            this.ExternalBayChainEnd();
                            return true;
                        }

                        this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitSource);
                        var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                        if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                        {
                            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                        }
                        if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                        {
                            this.Logger.LogInformation($"OpenShutter start Mission:Id={this.Mission.Id}");
                            this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                        }
                    }
                    else
                    {
                        this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
                    }
                    //if (this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                    //{
                    //    // No movement for external bay is required
                    //}
                }
            }

            if (this.Mission.ErrorCode == MachineErrorCode.MoveExtBayNotAllowed)
            {
                this.SetErrorMoveExtBayChain(bay, destination);
                return true;
            }
            this.Mission.Status = MissionStatus.Executing;

            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay,
                this.Mission.Step.ToString(),
                (this.Mission.Status == MissionStatus.Waiting) ? MessageStatus.OperationEnd : MessageStatus.OperationExecuting);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.ExternalBayStatus(notification);

            var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);
            var destination = bay.Positions.FirstOrDefault();
            var isLoadUnitDestinationInBay = (destination.Location == this.Mission.LoadUnitDestination);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (this.Mission.Status == MissionStatus.Executing
                        && (notification.Type == MessageType.ShutterPositioning
                            || notification.RequestingBay == this.Mission.TargetBay
                            )
                        )
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);

                            if (notification.Type == MessageType.Positioning &&
                                notification.TargetBay == notification.RequestingBay)
                            {
                                if (destination is null)
                                {
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                                }

                                //if (loadingUnitLocation == this.Mission.LoadUnitDestination)
                                //{
                                //    // Set the transaction only in this case
                                //    using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
                                //    {
                                //        //x this.BaysDataProvider.SetLoadingUnit(origin.Id, null);
                                //        //this.BaysDataProvider.SetLoadingUnit(destination.Id, this.Mission.LoadUnitId);
                                //        //transaction.Commit();
                                //    }
                                //}

                                if (this.Mission.RestoreConditions)
                                {
                                    this.LoadingUnitMovementProvider.UpdateLastBayChainPosition(this.Mission.TargetBay);
                                    this.Mission.RestoreConditions = false;
                                    this.Mission.ErrorCode = MachineErrorCode.NoError;

                                    this.Logger.LogDebug($"Restore conditions: {this.Mission.RestoreConditions}");
                                    //this.MissionsDataProvider.Update(this.Mission);
                                }

                                //// Execute the homing for bay, if required
                                //if (this.Mission.NeedHomingAxis == Axis.BayChain
                                //    && !isLoadUnitDestinationInBay
                                //    )
                                //{
                                //    this.MissionsDataProvider.Update(this.Mission);

                                //    this.Logger.LogInformation($"Homing External Bay Start Mission:Id={this.Mission.Id}");
                                //    this.LoadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadUnitId, true, notification.RequestingBay, MessageActor.MachineManager);
                                //}
                            }
                        }

                        // Handle the operation end homing operation
                        if (notification.Type == MessageType.Homing &&
                            notification.Data is HomingMessageData messageData)
                        {
                            // Calibrate horizontal axis
                            if (messageData.AxisToCalibrate == Axis.Horizontal &&
                                !this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                            {
                                this.MachineVolatileDataProvider.IsHomingExecuted = true;
                            }

                            // Calibrate external bay
                            if (messageData.AxisToCalibrate == Axis.BayChain)
                            {
                                this.Logger.LogDebug($"Homing full BayChain executed: prepare for empty homing");
                                this.Mission.NeedHomingAxis = Axis.BayChain;

                                this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;

                                if (isLoadUnitDestinationInBay && this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                                {
                                    this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
                                    this.Mission.LoadUnitDestination = destination.Location;

                                    this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                                    this.Logger.LogDebug($"Execute the move external bay toward Operator, DeviceNotification: {this.Mission.DeviceNotifications}");

                                    this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                                    var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                                    if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                                    {
                                        this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                                    }
                                    if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified)
                                    {
                                        this.Logger.LogInformation($"CloseShutter start Mission:Id={this.Mission.Id}");
                                        this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                                    }
                                }
                                else
                                {
                                    this.Logger.LogDebug($"Simulate positioning end");

                                    this.UpdateResponseList(MessageType.Positioning);
                                    this.MissionsDataProvider.Update(this.Mission);
                                }
                            }
                        }

                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified
                                && shutterPosition != this.Mission.OpenShutterPosition)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error);
                                break;
                            }
                            if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified
                                && shutterPosition != this.Mission.CloseShutterPosition)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error);
                                break;
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    if (this.Mission.Status == MissionStatus.Executing
                        && (notification.RequestingBay == this.Mission.TargetBay || notification.RequestingBay == BayNumber.None)
                        )
                    {
                        this.OnStop(StopRequestReason.Error);
                        return;
                    }
                    break;
            }

            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                && (
                    (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified && this.Mission.CloseShutterPosition == ShutterPosition.NotSpecified)
                        || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                    )
            {
                //if (this.Mission.NeedHomingAxis == Axis.BayChain &&
                //    !this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Homing))
                //{
                //    this.Logger.LogDebug($"Waiting for homing Mission:Id={this.Mission.Id}");
                //}
                //else
                if (this.Mission.ErrorCode == MachineErrorCode.MoveExtBayNotAllowed)
                {
                    this.SetErrorMoveExtBayChain(bay, bay.Positions.FirstOrDefault());
                }
                else
                {
                    if (isLoadUnitDestinationInBay && this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                    {
                        this.Logger.LogDebug($"4a. Move external bay toward Operator, mainly to handle the drawer after a restore conditions movement");

                        this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
                        //this.Mission.LoadUnitDestination = destination.Location;

                        this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    }
                    else if (!isLoadUnitDestinationInBay && this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number))
                    {
                        this.Logger.LogDebug($"5a. Move external bay toward Machine, mainly to handle the drawer after a restore conditions movement");

                        this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
                        //this.Mission.LoadUnitDestination = destination.Location;

                        this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    }
                    else
                    {
                        this.Logger.LogDebug($"4. Call the ExternalBayChainEnd, DeviceNotification: {this.Mission.DeviceNotifications}");
                        this.ExternalBayChainEnd();
                    }
                }
            }
        }

        private void ExternalBayChainEnd()
        {
            IMissionMoveBase newStep;
            if (this.CheckMissionShowError())
            {
                this.WakeUpMission();

                this.Logger.LogDebug($"Detect an error :: go to MissionMoveEndStep");

                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            //else if (this.Mission.MissionType == MissionType.OUT ||
            //    this.Mission.MissionType == MissionType.WMS ||
            //    this.Mission.MissionType == MissionType.FullTestOUT)
            //{
            //    this.WakeUpMission();

            //    this.Logger.LogDebug($"Go to MissionMoveWaitPickStep, Mission Type: {this.Mission.MissionType}");

            //    newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            //}
            else
            {
                var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);
                var destination = bay.Positions.FirstOrDefault();
                var isLoadUnitDestinationInBay = (destination.Location == this.Mission.LoadUnitDestination);

                if (isLoadUnitDestinationInBay)
                {
                    if (this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)
                        && !this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)
                        )
                    {
                        this.Logger.LogDebug($"1. Go to MissionMoveEndStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");
                        if (this.Mission.MissionType == MissionType.OUT ||
                            this.Mission.MissionType == MissionType.WMS ||
                            this.Mission.MissionType == MissionType.FullTestOUT)
                        {
                            newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                        else
                        {
                            newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                    }
                    else
                    {
                        // Detect an error condition
                        this.Logger.LogDebug($"2. Go to MissionMoveErrorStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");

                        this.ErrorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, this.Mission.TargetBay);
                        this.Mission.RestoreStep = this.Mission.Step;
                        newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }
                else
                {
                    if (this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)
                        && !this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)
                        )
                    {
                        this.Logger.LogDebug($"3. Go to MissionMoveLoadElevatorStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");
                        this.BaysDataProvider.IncrementCycles(bay.Number);
                        if (this.Mission.NeedHomingAxis == Axis.None
                            && bay.TotalCycles - bay.LastCalibrationCycles >= bay.CyclesToCalibrate
                            )
                        {
                            this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;
                        }

                        newStep = new MissionMoveLoadElevatorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        // Detect an error condition
                        this.Logger.LogDebug($"4. Go to MissionMoveErrorStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");

                        this.ErrorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, this.Mission.TargetBay);
                        this.Mission.RestoreStep = this.Mission.Step;
                        newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }
            }
            newStep.OnEnter(null);
        }

        private void SetErrorMoveExtBayChain(Bay bay, BayPosition position)
        {
            this.ErrorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, bay.Number);

            //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
            this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(this.Mission.TargetBay);
            this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
            this.BaysDataProvider.Light(this.Mission.TargetBay, true);

            this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
            this.Mission.RestoreStep = this.Mission.Step;

            var newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        private void WakeUpMission()
        {
            var waitingMission = this.MissionsDataProvider.GetAllActiveMissions()
                .FirstOrDefault(m => m.LoadUnitSource == this.Mission.LoadUnitDestination
                    && (m.Step == MissionStep.WaitDepositCell || m.Step == MissionStep.WaitChain));

            if (waitingMission != null)
            {
                // wake up the mission waiting for the bay chain movement
                this.Logger.LogInformation($"Resume waiting deposit Mission:Id={waitingMission.Id}");
                this.LoadingUnitMovementProvider.ResumeOperation(
                    waitingMission.Id,
                    waitingMission.LoadUnitSource,
                    waitingMission.LoadUnitDestination,
                    waitingMission.WmsId,
                    waitingMission.MissionType,
                    waitingMission.TargetBay,
                    MessageActor.MachineManager);
            }
        }

        #endregion
    }
}
