using System;
using System.Collections.Generic;
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

        public override bool OnEnter(CommandMessage command)
        {
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.End;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.BayNotifications = MissionBayNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.StopReason == StopRequestReason.NoReason)
            {
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

        private void SendNotification()
        {
            var notificationText = $"Load Unit {this.Mission.LoadUnitId} end movement to bay {this.Mission.LoadUnitDestination}";
            this.SendMoveNotification(this.Mission.TargetBay, notificationText, MessageStatus.OperationEnd);
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
