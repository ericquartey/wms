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
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.ExtBay;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            //this.Mission.CloseShutterBayNumber = BayNumber.None;
            //this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            //this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var loadingUnitLocation = (this.Mission.LoadUnitDestination is LoadingUnitLocation.Elevator || this.Mission.LoadUnitDestination is LoadingUnitLocation.Cell) ?
                this.Mission.LoadUnitSource :
                this.Mission.LoadUnitDestination;

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(/*this.Mission.LoadUnitDestination*/loadingUnitLocation);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew((loadingUnitLocation == this.Mission.LoadUnitDestination) ? MachineErrorCode.LoadUnitDestinationBay : MachineErrorCode.LoadUnitSourceBay /*MachineErrorCode.LoadUnitDestinationBay*/, this.Mission.TargetBay);
                throw new StateMachineException((loadingUnitLocation == this.Mission.LoadUnitDestination) ? ErrorDescriptions.LoadUnitDestinationBay : ErrorDescriptions.LoadUnitSourceBay /*ErrorDescriptions.LoadUnitDestinationBay*/, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            var destination = bay.Positions.FirstOrDefault();
            //if (destination is null)
            //{
            //    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
            //    throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
            //}
            this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);
            if (this.Mission.RestoreConditions
                //&& this.LoadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number)
                && Math.Abs(this.BaysDataProvider.GetChainPosition(bay.Number) - bay.External.LastIdealPosition) > Math.Abs(bay.ChainOffset) + 1
                )
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.AutomaticRestoreNotAllowed, bay.Number);
                throw new StateMachineException(ErrorDescriptions.AutomaticRestoreNotAllowed, bay.Number, MessageActor.MachineManager);
            }
            var position = bay.Positions.FirstOrDefault();
            //if (position != null
            //    && position.LoadingUnit != null
            //    && position.LoadingUnit.Id == this.Mission.LoadUnitId
            //    && !this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
            //    && this.LoadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number)
            //    && (position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare) > (position.LoadingUnit.MaxNetWeight * LoadUnitMaxNetWeightBayChainPercent)
            //    )
            //{
            //    this.Mission.ErrorCode = MachineErrorCode.MoveBayChainNotAllowed;
            //}

            if (position != null &&
                position.LoadingUnit != null &&
                position.LoadingUnit.Id == this.Mission.LoadUnitId &&
                !this.SensorsProvider.IsLoadingUnitInLocation(destination.Location) &&
                (loadingUnitLocation == this.Mission.LoadUnitDestination && this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)) ||
                (loadingUnitLocation != this.Mission.LoadUnitDestination && this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)))
            {
                this.Mission.ErrorCode = MachineErrorCode.MoveBayChainNotAllowed;   // Maybe define a MachineErrorCode.MoveExtBayNotAllowed
            }

            try
            {
                if (this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == destination.Location && m.Id != this.Mission.Id))
                {
                    throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, bay.Number, MessageActor.MachineManager);
                }

                if (this.Mission.ErrorCode != MachineErrorCode.MoveBayChainNotAllowed)
                {
                    var isLoadUnitDestinationInBay = this.Mission.LoadUnitDestination /*loadingUnitLocation*/ == LoadingUnitLocation.InternalBay1Up ||
                        this.Mission.LoadUnitDestination /*loadingUnitLocation*/ == LoadingUnitLocation.InternalBay2Up ||
                        this.Mission.LoadUnitDestination /*loadingUnitLocation*/ == LoadingUnitLocation.InternalBay3Up;

                    if (isLoadUnitDestinationInBay)
                    {
                        if (this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                        {
                            this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
                            this.Mission.LoadUnitDestination = destination.Location;
                        }
                        if (this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number))
                        {
                            // No movement for external bay is required
                        }

                        //var shutterInverter = bay.Shutter.Inverter.Index;
                        //if (bay.Shutter.Type == ShutterType.TwoSensors &&
                        //    this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed)
                        //{
                        //    this.Logger.LogInformation($"Close Shutter Mission:Id={this.Mission.Id}");
                        //    this.Mission.CloseShutterPosition = ShutterPosition.Closed;
                        //    this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false);
                        //}
                    }
                    else
                    {
                        if (this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number))
                        {
                            this.LoadingUnitMovementProvider.MoveExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
                            //this.Mission.LoadUnitDestination = destination.Location;
                        }
                        if (this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                        {
                            // No movement for external bay is required
                        }

                        //var shutterInverter = bay.Shutter.Inverter.Index;
                        //if (bay.Shutter.Type == ShutterType.TwoSensors &&
                        //    this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Opened)
                        //{
                        //    this.Logger.LogInformation($"Open Shutter Mission:Id={this.Mission.Id}");
                        //    this.Mission.OpenShutterPosition = ShutterPosition.Opened;
                        //    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                        //}
                    }
                }

                //var shutterInverter = bay.Shutter.Inverter.Index;
                //if (bay.Shutter.Type == ShutterType.TwoSensors &&
                //    this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Closed)
                //{
                //    this.Logger.LogInformation($"Close Shutter Mission:Id={this.Mission.Id}");
                //    this.Mission.CloseShutterPosition = ShutterPosition.Closed;
                //    this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false);
                //}
                /*else*/
                if (this.Mission.ErrorCode == MachineErrorCode.MoveBayChainNotAllowed)
                {
                    this.SetErrorMoveExtBayChain(bay, position);
                    return true;
                }
                this.Mission.Status = MissionStatus.Executing;
            }
            catch (StateMachineException ex)
            {
                var description = $"Move External Bay chain not allowed at the moment in bay {bay.Number}. Reason {ex.NotificationMessage.Description}. Wait for resume";
                // we don't want any exception here because this is the normal procedure:
                // send a second LU in lower position while operator is working on upper position
                this.Logger.LogInformation(description);
                this.Mission.Status = MissionStatus.Waiting;
            }

            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay,
                this.Mission.Step.ToString(),
                (this.Mission.Status == MissionStatus.Waiting) ? MessageStatus.OperationEnd : MessageStatus.OperationExecuting);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.ExternalBayStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (this.Mission.Status == MissionStatus.Executing &&
                        /*(notification.Type == MessageType.ShutterPositioning ||*/ notification.RequestingBay == this.Mission.TargetBay)
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);

                            var loadingUnitLocation = (this.Mission.LoadUnitDestination is LoadingUnitLocation.Elevator || this.Mission.LoadUnitDestination is LoadingUnitLocation.Cell) ?
                                this.Mission.LoadUnitSource :
                                this.Mission.LoadUnitDestination;

                            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(/*this.Mission.LoadUnitDestination*/loadingUnitLocation);
                            if (notification.Type == MessageType.Positioning &&
                                notification.TargetBay == notification.RequestingBay)
                            {
                                var destination = bay.Positions.FirstOrDefault();
                                if (destination is null)
                                {
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                                }
                                //var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                                //if (origin is null)
                                //{
                                //    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedBottom, this.Mission.TargetBay);
                                //    throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedBottom, this.Mission.TargetBay, MessageActor.MachineManager);
                                //}

                                if (loadingUnitLocation == this.Mission.LoadUnitDestination)
                                {
                                    // Set the transaction only in this case
                                    using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
                                    {
                                        //x this.BaysDataProvider.SetLoadingUnit(origin.Id, null);
                                        //this.BaysDataProvider.SetLoadingUnit(destination.Id, this.Mission.LoadUnitId);
                                        //transaction.Commit();
                                    }
                                }

                                //var notificationText = $"Load Unit {this.Mission.LoadUnitId} placed on bay {bay.Number}";
                                //this.SendMoveNotification(bay.Number, notificationText, MessageStatus.OperationWaitResume);

                                if (this.Mission.RestoreConditions)
                                {
                                    this.LoadingUnitMovementProvider.UpdateLastBayChainPosition(this.Mission.TargetBay);
                                    this.Mission.RestoreConditions = false;
                                }

                                if (this.Mission.NeedHomingAxis == Axis.BayChain /*&&*/
                                   /* bay.Positions.Count(p => p.LoadingUnit != null) < 2*/)
                                {
                                    this.MissionsDataProvider.Update(this.Mission);
                                    this.Logger.LogInformation($"Homing Bay occupied start Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadUnitId, true, notification.RequestingBay, MessageActor.MachineManager);
                                }
                            }
                        }

                        //if (notification.Type == MessageType.ShutterPositioning)
                        //{
                        //    var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                        //    var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);

                        //    var isLoadUnitDestinationInBay = this.Mission.LoadUnitDestination == LoadingUnitLocation.InternalBay1Up ||
                        //        this.Mission.LoadUnitDestination == LoadingUnitLocation.InternalBay2Up ||
                        //        this.Mission.LoadUnitDestination == LoadingUnitLocation.InternalBay3Up;

                        //    // Check the actual position of shutter
                        //    if (isLoadUnitDestinationInBay)
                        //    {
                        //        if (shutterPosition != this.Mission.CloseShutterPosition)
                        //        {
                        //            this.Logger.LogError(ErrorDescriptions.LoadUnitShutterOpen);
                        //            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, notification.RequestingBay);

                        //            this.OnStop(StopRequestReason.Error, !this.ErrorsProvider.IsErrorSmall());
                        //            return;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (shutterPosition != this.Mission.OpenShutterPosition)
                        //        {
                        //            this.Logger.LogError(ErrorDescriptions.LoadUnitShutterClosed);
                        //            this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);

                        //            this.OnStop(StopRequestReason.Error, !this.ErrorsProvider.IsErrorSmall());
                        //            return;
                        //        }
                        //    }
                        //}

                        if (notification.Type == MessageType.Homing && notification.Data is HomingMessageData messageData)
                        {
                            // Calibrate horizontal axis
                            if (messageData.AxisToCalibrate == Axis.Horizontal &&
                                !this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                            {
                                this.MachineVolatileDataProvider.IsHomingExecuted = true;
                            }

                            // Calibrate external bay
                            if (messageData.AxisToCalibrate == Axis.BayChain /*&&*/
                                /*this.LoadingUnitMovementProvider.IsOnlyTopPositionOccupied(this.Mission.TargetBay)*/)
                            {
                                this.ExternalBayChainEnd();
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    if (this.Mission.Status == MissionStatus.Executing && notification.RequestingBay == this.Mission.TargetBay)
                    {
                        this.OnStop(StopRequestReason.Error);
                        return;
                    }
                    break;
            }

            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning) /*&&*/
                 //(this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified ||
                 /*this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))*/)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                if (this.Mission.NeedHomingAxis == Axis.BayChain &&
                    /*bay.Positions.Count(p => p.LoadingUnit != null) < 2 &&*/
                    !this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Homing))
                {
                    this.Logger.LogDebug($"Waiting for homing Mission:Id={this.Mission.Id}");
                }
                else if (this.Mission.ErrorCode == MachineErrorCode.MoveBayChainNotAllowed)
                {
                    this.SetErrorMoveExtBayChain(bay, bay.Positions.FirstOrDefault());
                }
                else
                {
                    this.ExternalBayChainEnd();
                }
            }
        }

        private void ExternalBayChainEnd()
        {
            IMissionMoveBase newStep;
            if (this.CheckMissionShowError())
            {
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            else if (this.Mission.MissionType == MissionType.OUT
                || this.Mission.MissionType == MissionType.WMS
                || this.Mission.MissionType == MissionType.FullTestOUT
                )
            {
                var waitingMission = this.MissionsDataProvider.GetAllActiveMissions()
                    .FirstOrDefault(m => m.LoadUnitSource == this.Mission.LoadUnitDestination
                        && (m.Step == MissionStep.WaitDeposit || m.Step == MissionStep.WaitChain));

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

                newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            else
            {
                var loadingUnitLocation = (this.Mission.LoadUnitDestination is LoadingUnitLocation.Elevator || this.Mission.LoadUnitDestination is LoadingUnitLocation.Cell) ?
                    this.Mission.LoadUnitSource :
                    this.Mission.LoadUnitDestination;

                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(/*this.Mission.LoadUnitDestination*/loadingUnitLocation);
                var isLoadUnitDestinationInBay = this.Mission.LoadUnitDestination /*loadingUnitLocation*/ == LoadingUnitLocation.InternalBay1Up ||
                    this.Mission.LoadUnitDestination /*loadingUnitLocation*/ == LoadingUnitLocation.InternalBay2Up ||
                    this.Mission.LoadUnitDestination /*loadingUnitLocation*/ == LoadingUnitLocation.InternalBay3Up;

                if (isLoadUnitDestinationInBay)
                {
                    if (this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number))
                    {
                        newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        // ?
                        newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }
                else
                {
                    if (this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number))
                    {
                        newStep = new MissionMoveLoadElevatorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        // ?
                        newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }

                //newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            newStep.OnEnter(null);
        }

        private void SetErrorMoveExtBayChain(Bay bay, BayPosition position)
        {
            //this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed,
            //    bay.Number,
            //    string.Format(Resources.Missions.RemoveMaterialFromLoadUnit,
            //        Math.Round((position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare) - (position.LoadingUnit.MaxNetWeight * LoadUnitMaxNetWeightBayChainPercent)),
            //        this.Mission.LoadUnitId,
            //        Math.Round(position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare)));
            this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed, bay.Number);

            this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
            this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
            this.BaysDataProvider.Light(this.Mission.TargetBay, true);

            // set gross weight to the maximum that do not show this error again
            //this.LoadingUnitsDataProvider.SetWeight(this.Mission.LoadUnitId, position.LoadingUnit.MaxNetWeight + position.LoadingUnit.Tare);

            this.Mission.ErrorCode = MachineErrorCode.MoveBayChainNotAllowed; //MachineErrorCode.LoadUnitWeightExceeded;
            this.Mission.RestoreStep = this.Mission.Step;

            var newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        #endregion
    }
}
