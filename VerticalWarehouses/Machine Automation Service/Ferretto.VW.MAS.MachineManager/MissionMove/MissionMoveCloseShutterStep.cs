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
    public class MissionMoveCloseShutterStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveCloseShutterStep(Mission mission,
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
            this.Mission.Step = MissionStep.CloseShutter;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }

            var executeShutterClosing = true;
            if (bay.IsDouble && (bay.Carousel == null) && !bay.IsExternal)
            {
                // Detect if a waiting mission exists
                // Only applied for the internal double bay configuration
                var isAtLeastOneWaitingMission = this.isWaitingMissionOnThisBay(bay);
                if (isAtLeastOneWaitingMission &&
                   (this.Mission.MissionType == MissionType.OUT ||
                    this.Mission.MissionType == MissionType.WMS ||
                    this.Mission.MissionType == MissionType.FullTestOUT))
                {
                    // Check if bay is an internal double Bay (not carousel bay or external bay), the mission type is OUT, WMS, ... and
                    // check if there is at least one waiting mission different from the current one
                    executeShutterClosing = false;
                }
            }

            if (executeShutterClosing)
            {
                // Close the shutter of specified bay
                this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, this.Mission.CloseShutterPosition);

                var machine = this.MachineProvider.Get();
                if (this.Mission.NeedHomingAxis == Axis.Horizontal
                    || (this.Mission.NeedHomingAxis == Axis.None
                        && (Math.Abs(this.LoadingUnitMovementProvider.GetCurrentHorizontalPosition()) >= machine.HorizontalPositionToCalibrate
                            || this.LoadingUnitMovementProvider.GetCyclesFromCalibration() >= machine.HorizontalCyclesToCalibrate
                            )
                        )
                    )
                {
                    this.Mission.NeedHomingAxis = Axis.Horizontal;
                    this.Logger.LogInformation($"Homing horizontal elevator start Mission:Id={this.Mission.Id}");
                    this.LoadingUnitMovementProvider.Homing(this.Mission.NeedHomingAxis, Calibration.FindSensor, this.Mission.LoadUnitId, true, this.Mission.TargetBay, MessageActor.MachineManager);
                }

                this.Mission.RestoreConditions = false;
                this.MissionsDataProvider.Update(this.Mission);

                this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            }
            else
            {
                // Not close the shutter and go ahead to the WaitPick step
                var newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.ShutterStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.Homing
                        && notification.Data is HomingMessageData messageData
                        )
                    {
                        this.OnHomingNotification(messageData);
                    }
                    else if (notification.Type == MessageType.ShutterPositioning)
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                            this.Logger.LogTrace($"UpdateResponseList: {notification.Type} Mission:Id={this.Mission.Id}");
                        }
                    }
                    if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter)
                        && this.Mission.NeedHomingAxis != Axis.Horizontal
                        )
                    {
                        this.CloseShutterEnd();
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

        private void CloseShutterEnd()
        {
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (this.Mission.ErrorCode != MachineErrorCode.NoError
                && !this.isWaitingMissionOnThisBay(bay)
                )
            {
                //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(this.Mission.TargetBay);
                this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
                this.Logger.LogInformation($"Display error for Mission.Id={this.Mission.Id}, ErrorCode={this.Mission.ErrorCode}");
                this.ErrorsProvider.RecordNew(this.Mission.ErrorCode, this.Mission.TargetBay);
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
            }
            IMissionMoveBase newStep;
            if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.LoadUnit
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.NoLocation
                && this.Mission.MissionType != MissionType.FirstTest
                )
            {
                if (this.Mission.MissionType == MissionType.Manual
                    || this.Mission.MissionType == MissionType.LoadUnitOperation
                    || this.Mission.MissionType == MissionType.ScaleCalibration)
                {
                    this.BaysDataProvider.Light(bay.Number, true);

                    if (bay.External != null)
                    {
                        newStep = new MissionMoveExtBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }
                else
                {
                    if (bay.IsDouble
                        && bay.Carousel is null
                        && this.Mission.ErrorCode != MachineErrorCode.NoError)
                    {
                        if (this.isWaitingMissionOnThisBay(bay))
                        {
                            this.Logger.LogInformation($"Mission.Id={this.Mission.Id}: Go to WaitPick step, there are waiting missions.");
                            newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                        else
                        {
                            this.Logger.LogInformation($"Mission.Id={this.Mission.Id}: Go to End step, there are no waiting missions.");
                            newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                    }
                    else if (bay.Positions.Count() == 1
                        || bay.Positions.Any(x => x.Location == this.Mission.LoadUnitDestination && x.IsUpper)
                        || bay.Positions.Any(x => x.IsUpper && x.IsBlocked)
                        || bay.Carousel is null)
                    {
                        if (bay.External != null)
                        {
                            // External bay movement
                            newStep = new MissionMoveExtBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                        else
                        {
                            if (this.Mission.MissionType == MissionType.OUT
                                || this.Mission.MissionType == MissionType.WMS
                                || this.Mission.MissionType == MissionType.FullTestOUT
                                )
                            {
                                newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            }
                            else
                            {
                                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            }
                        }
                    }
                    else
                    {
                        // Carousel movement
                        newStep = new MissionMoveBayChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }
            }
            else
            {
                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            newStep.OnEnter(null);
        }

        /// <summary>
        /// Check if exist at least a waiting mission (step == MissionStep.WaitPick) in the current bay.
        /// Applied only for double bay.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if exists at least a waiting mission,
        ///     <c>false</c> otherwise.
        /// </returns>
        private bool isWaitingMissionOnThisBay(Bay bay)
        {
            var retValue = false;

            if (bay != null)
            {
                if (bay.IsDouble)
                {
                    // List of waiting mission on the bay
                    var waitMissions = this.MissionsDataProvider.GetAllMissions()
                        .Where(
                            m => m.LoadUnitId != this.Mission.LoadUnitId &&
                            m.Id != this.Mission.Id &&
                            (m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick)
                        );

                    retValue = waitMissions.Any();
                }
            }

            return retValue;
        }

        #endregion
    }
}
