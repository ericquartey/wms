using System;
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
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.End;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.BayNotifications = MissionBayNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.StopReason == StopRequestReason.NoReason)
            {
                this.Logger.LogInformation($"End Move load unit {this.Mission.LoadUnitId} to {this.Mission.LoadUnitDestination} {this.Mission.DestinationCellId} on bay {this.Mission.TargetBay} ");

                this.Mission.Status = MissionStatus.Completed;
                this.MissionsDataProvider.Update(this.Mission);

                this.SendNotification();
            }
            else
            {
                var stopMessageData = new StopMessageData(this.Mission.StopReason);
                this.LoadingUnitMovementProvider.StopOperation(stopMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                this.Mission.RestoreConditions = false;
                this.MissionsDataProvider.Update(this.Mission);

                this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            }

            if (this.Mission.MissionType == MissionType.LoadUnitOperation)
            {
                this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
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

                var bays = this.BaysDataProvider.GetAll();
                if (this.Mission != null
                    && this.AllStopped(bays)
                    )
                {
                    this.Mission.BayNotifications = MissionBayNotifications.None;
                    this.Mission.Status = MissionStatus.Aborted;
                    this.MissionsDataProvider.Update(this.Mission);
                    this.SendNotification();
                }
            }
        }

        public override void OnStop(StopRequestReason reason, bool moveBackward = false)
        {
            if (this.Mission.StopReason != StopRequestReason.NoReason)
            {
                this.Mission.Status = MissionStatus.Aborted;
                this.MissionsDataProvider.Update(this.Mission);
                this.SendNotification();
            }
        }

        private bool AllStopped(IEnumerable<Bay> bays)
        {
            var stopped = MissionBayNotifications.None;
            foreach (var bay in bays)
            {
                switch (bay.Number)
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

        private void checkOtherMissionsOnCurrentBay()
        {
            // ???? ///
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
                        var mission = waitMissions.FirstOrDefault();
                        // this.SendMoveNotification(mission.TargetBay, mission.Step.ToString(), MessageStatus.OperationEnd);

                        this.Logger.LogDebug($"Show the error mission: reason {mission.ErrorCode}");
                        // Show the error message (reserved for LoadUnitWeightExceeded)
                        this.CheckMissionShowError(mission);

                        // Send information about the mission with LoadUnitWeightExceeded
                        var messageData = new MoveLoadingUnitMessageData(
                            mission.MissionType,
                            mission.LoadUnitSource,
                            mission.LoadUnitDestination,
                            mission.LoadUnitCellSourceId,
                            mission.DestinationCellId,
                            mission.LoadUnitId,
                            (mission.LoadUnitDestination == LoadingUnitLocation.Cell),
                            mission.Id,
                            mission.Action,
                            mission.StopReason,
                            mission.Step);

                        this.Logger.LogDebug($"Send a notification message to the MissionManager: loading unit Id {mission.LoadUnitId} movement is completed");
                        var msg = new NotificationMessage(
                            messageData,
                            mission.Step.ToString(),
                            MessageActor.AutomationService,
                            MessageActor.MachineManager,
                            MessageType.MoveLoadingUnit,
                            mission.TargetBay,
                            mission.TargetBay,
                            MessageStatus.OperationEnd);
                        this.EventAggregator.GetEvent<Ferretto.VW.MAS.Utils.Events.NotificationEvent>().Publish(msg);
                    }
                }
            }
        }

        private void SendNotification()
        {
            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationEnd);

            // -------------------------------------
            // Add this code...
            this.Logger.LogDebug($"Check the missions in a double internal bay with mission error code = Exceeded weight in the loading unit");
            this.checkOtherMissionsOnCurrentBay();
            // -------------------------------------
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
