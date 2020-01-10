using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveEndState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveEndState(Mission mission,
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

        public override bool OnEnter(CommandMessage command)
        {
            this.Mission.FsmRestoreStateName = null;
            this.Mission.FsmStateName = nameof(MissionMoveEndState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.BayNotifications = MissionBayNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            if (this.Mission.StopReason == StopRequestReason.NoReason)
            {
                this.SendNotification();
            }
            else
            {
                var stopMessageData = new StopMessageData(this.Mission.StopReason);
                this.LoadingUnitMovementProvider.StopOperation(stopMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                this.Mission.RestoreConditions = false;
                this.MissionsDataProvider.Update(this.Mission);
            }

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
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
                    this.MissionsDataProvider.Update(this.Mission);
                    this.SendNotification();
                }
            }
        }

        public override void OnStop(StopRequestReason reason, bool moveBackward = false)
        {
            // no action
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
                }
            }
            return (stopped == this.Mission.BayNotifications);
        }

        private void SendNotification()
        {
            bool isEject = this.Mission.LoadingUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.LoadingUnit
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.NoLocation;

            var notificationText = $"Load Unit {this.Mission.LoadingUnitId} end movement to bay {this.Mission.LoadingUnitDestination}";
            this.SendMoveNotification(this.Mission.TargetBay, notificationText, isEject, StopRequestReasonConverter.GetMessageStatusFromReason(this.Mission.StopReason));
        }

        private bool UpdateStopList(BayNumber bay)
        {
            bool update = false;
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
            }
            return update;
        }

        #endregion
    }
}
