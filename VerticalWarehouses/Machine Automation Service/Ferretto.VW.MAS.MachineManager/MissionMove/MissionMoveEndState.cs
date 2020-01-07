using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveEndState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionMoveEndState(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.loadingUnitMovementProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitMovementProvider>();

            this.logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command)
        {
            var stopReason = StopRequestReason.NoReason;
            if (command != null
                && command.Data is IMoveLoadingUnitMessageData messageData
                )
            {
                stopReason = messageData.StopReason;
            }
            this.Mission.FsmStateName = nameof(MissionMoveEndState);
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            bool isEject = this.Mission.LoadingUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.LoadingUnit
                && this.Mission.LoadingUnitDestination != LoadingUnitLocation.NoLocation;

            var newMessageData = new MoveLoadingUnitMessageData(
                this.Mission.MissionType,
                this.Mission.LoadingUnitSource,
                this.Mission.LoadingUnitDestination,
                this.Mission.LoadingUnitCellSourceId,
                this.Mission.DestinationCellId,
                this.Mission.LoadingUnitId,
                (this.Mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                isEject,
                this.Mission.FsmId,
                this.Mission.Action,
                stopReason);

            var msg = new NotificationMessage(
                newMessageData,
                $"Loading Unit {this.Mission.LoadingUnitId} start movement to bay {this.Mission.LoadingUnitDestination}",
                MessageActor.AutomationService,
                MessageActor.MachineManager,
                MessageType.MoveLoadingUnit,
                this.Mission.TargetBay,
                this.Mission.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(stopReason));
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

            if (stopReason == StopRequestReason.NoReason)
            {
                if (this.Mission != null)
                {
                    this.missionsDataProvider.Delete(this.Mission.Id);
                }
            }
            else
            {
                var stopMessageData = new StopMessageData(stopReason);
                this.loadingUnitMovementProvider.StopOperation(stopMessageData, BayNumber.All, MessageActor.MachineManager, this.Mission.TargetBay);
                this.Mission.RestoreConditions = false;
                this.missionsDataProvider.Update(this.Mission);
            }

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.loadingUnitMovementProvider.StopOperationStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    // State machine is in error, any response from device manager state machines will do to complete state machine shutdown
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationRunningStop:
                        this.UpdateStopList(notification.TargetBay);
                        break;
                }

                var bays = this.baysDataProvider.GetAll();
                if (this.Mission != null
                    && this.AllStopped(bays)
                    )
                {
                    this.missionsDataProvider.Delete(this.Mission.Id);
                }
            }
        }

        public override void OnStop(StopRequestReason reason)
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

        private void UpdateStopList(BayNumber bay)
        {
            switch (bay)
            {
                case BayNumber.BayOne:
                    this.Mission.BayNotifications |= MissionBayNotifications.BayOne;
                    break;

                case BayNumber.BayTwo:
                    this.Mission.BayNotifications |= MissionBayNotifications.BayTwo;
                    break;

                case BayNumber.BayThree:
                    this.Mission.BayNotifications |= MissionBayNotifications.BayThree;
                    break;
            }
        }

        #endregion
    }
}
