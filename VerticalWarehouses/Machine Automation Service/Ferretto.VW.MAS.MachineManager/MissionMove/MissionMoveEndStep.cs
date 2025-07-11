﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveEndStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveEndStep(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
            // do nothing
        }

        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.End;
            this.Mission.MissionTime = this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.BayNotifications = MissionBayNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.NeedHomingAxis == Axis.Horizontal)
            {
                this.MachineVolatileDataProvider.IsHomingExecuted = false;
            }

            if (this.Mission.StopReason == StopRequestReason.NoReason)
            {
                this.Logger.LogInformation($"End Move load unit {this.Mission.LoadUnitId} to {this.Mission.LoadUnitDestination} {this.Mission.DestinationCellId} on bay {this.Mission.TargetBay} ");

                this.Mission.Status = MissionStatus.Completed;
                this.MissionsDataProvider.Update(this.Mission);

                this.MachineProvider.UpdateMissionTime(this.Mission.MissionTime);
                this.SendNotification();
            }
            else
            {
                var stopMessageData = new StopMessageData(this.Mission.StopReason);
                this.LoadingUnitMovementProvider.StopOperation(stopMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                this.Mission.RestoreConditions = false;
                this.Mission.Status = MissionStatus.Executing;
                this.MissionsDataProvider.Update(this.Mission);

                this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            }

            if (this.Mission.MissionType == MissionType.LoadUnitOperation)
            {
                //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(this.Mission.TargetBay);
                this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
            }

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            this.Logger.LogTrace($"type {notification.Type}, status {notification.Status}, target {notification.TargetBay}, request {notification.RequestingBay}");

            var notificationStatus = this.LoadingUnitMovementProvider.StopOperationStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    // State machine is in error, any response from device manager state machines will do to complete state machine shutdown
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationRunningStop:
                    case MessageStatus.OperationFaultStop:
                        if (this.UpdateStopList(notification.TargetBay))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                        }
                        break;
                }

                var bayNumbers = this.BaysDataProvider.GetBayNumbers();
                if (this.Mission != null
                    && this.AllStopped(bayNumbers)
                    )
                {
                    this.Mission.BayNotifications = MissionBayNotifications.None;
                    this.Mission.Status = MissionStatus.Aborted;
                    this.Mission.MissionTime = this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
                    this.MissionsDataProvider.Update(this.Mission);

                    this.MachineProvider.UpdateMissionTime(this.Mission.MissionTime);
                    this.SendNotification();
                }
            }
        }

        public override void OnStop(StopRequestReason reason, bool moveBackward = false)
        {
            if (this.Mission.StopReason != StopRequestReason.NoReason)
            {
                this.Mission.Status = MissionStatus.Aborted;
                this.Mission.MissionTime = this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
                this.MissionsDataProvider.Update(this.Mission);

                this.MachineProvider.UpdateMissionTime(this.Mission.MissionTime);
                this.SendNotification();
            }
        }

        private bool AllStopped(IEnumerable<BayNumber> bayNumbers)
        {
            var stopped = MissionBayNotifications.None;
            foreach (var bayNumber in bayNumbers)
            {
                switch (bayNumber)
                {
                    case BayNumber.BayOne:
                        stopped |= MissionBayNotifications.BayOne;
                        break;

                    case BayNumber.BayTwo:
                        stopped |= MissionBayNotifications.BayTwo;
                        break;

                    case BayNumber.BayThree:
                        stopped |= MissionBayNotifications.BayThree;
                        break;

                    case BayNumber.ElevatorBay:
                        stopped |= MissionBayNotifications.ElevatorBay;
                        break;
                }
            }
            return (stopped == this.Mission.BayNotifications);
        }

        /// <summary>
        /// Check existence of error missions.
        /// Only applied for internal double bay.
        /// </summary>
        /// <returns></returns>
        private Mission checkOtherMissionsOnCurrentBay()
        {
            Mission retValue = null;

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
            if (!(bay is null))
            {
                // Check if bay is double internal bay
                if (bay.IsDouble && bay.Carousel == null && !bay.IsExternal)
                {
                    // List of waiting mission on the bay
                    var waitMissions = this.MissionsDataProvider.GetAllMissions()
                        .Where(m => m.LoadUnitId != this.Mission.LoadUnitId &&
                               m.Id != this.Mission.Id &&
                               m.Status == MissionStatus.Completed &&
                               m.Step == MissionStep.End &&
                               m.ErrorCode == MachineErrorCode.LoadUnitWeightExceeded);

                    if (waitMissions.Any())
                    {
                        retValue = waitMissions.FirstOrDefault();
                    }
                }
            }

            return retValue;
        }

        private void SendNotification()
        {
            // Detect if an error codition exist. The error condition is a MachineErrorCode.LoadUnitWeigthExceeded and it happens
            // in a internal double bay
            var missionError = this.checkOtherMissionsOnCurrentBay();
            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationEnd);

            if (missionError != null)
            {
                this.Logger.LogDebug($"Show the error mission: reason {missionError.ErrorCode}");
                // Show the error message (reserved for LoadUnitWeightExceeded)
                this.CheckMissionShowError(missionError);

                // Send information about the mission with LoadUnitWeightExceeded
                var loadUnit = this.LoadingUnitsDataProvider.GetById(this.Mission.LoadUnitId);
                var messageData = new MoveLoadingUnitMessageData(
                    missionError.MissionType,
                    missionError.LoadUnitSource,
                    missionError.LoadUnitDestination,
                    missionError.LoadUnitCellSourceId,
                    missionError.DestinationCellId,
                    missionError.LoadUnitId,
                    (missionError.LoadUnitDestination == LoadingUnitLocation.Cell),
                    missionError.Id,
                    (int?)loadUnit?.Height,
                    (int?)loadUnit?.NetWeight,
                    missionError.Action,
                    missionError.StopReason,
                    missionError.Step);

                this.Logger.LogDebug($"Send a notification message to the MissionManager: loading unit Id {missionError.LoadUnitId} movement is completed");
                var msg = new NotificationMessage(
                    messageData,
                    missionError.Step.ToString(),
                    MessageActor.AutomationService,
                    MessageActor.MachineManager,
                    MessageType.MoveLoadingUnit,
                    missionError.TargetBay,
                    missionError.TargetBay,
                    MessageStatus.OperationEnd);
                this.EventAggregator.GetEvent<Ferretto.VW.MAS.Utils.Events.NotificationEvent>().Publish(msg);
            }
        }

        private bool UpdateStopList(BayNumber bay)
        {
            var update = false;
            switch (bay)
            {
                case BayNumber.BayOne:
                    this.Mission.BayNotifications |= MissionBayNotifications.BayOne;
                    update = true;
                    break;

                case BayNumber.BayTwo:
                    this.Mission.BayNotifications |= MissionBayNotifications.BayTwo;
                    update = true;
                    break;

                case BayNumber.BayThree:
                    this.Mission.BayNotifications |= MissionBayNotifications.BayThree;
                    update = true;
                    break;

                case BayNumber.ElevatorBay:
                    this.Mission.BayNotifications |= MissionBayNotifications.ElevatorBay;
                    update = true;
                    break;
            }
            return update;
        }

        #endregion
    }
}
