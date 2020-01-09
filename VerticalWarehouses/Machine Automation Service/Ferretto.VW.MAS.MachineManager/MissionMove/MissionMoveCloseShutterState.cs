using System;
using System.Linq;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveCloseShutterState : MissionMoveBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionMoveCloseShutterState(Mission mission,
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
            this.Mission.FsmRestoreStateName = null;
            this.Mission.FsmStateName = nameof(MissionMoveCloseShutterState);
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
            if (bay is null)
            {
                var description = $"{this.GetType().Name}: destination bay not found {this.Mission.LoadingUnitDestination}";

                throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            this.loadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);

            this.Mission.RestoreConditions = false;
            this.missionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.loadingUnitMovementProvider.ShutterStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    bool isEject = this.Mission.LoadingUnitDestination != LoadingUnitLocation.Cell
                        && this.Mission.LoadingUnitDestination != LoadingUnitLocation.Elevator
                        && this.Mission.LoadingUnitDestination != LoadingUnitLocation.LoadingUnit
                        && this.Mission.LoadingUnitDestination != LoadingUnitLocation.NoLocation;
                    if (isEject)
                    {
                        var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                        var messageData = new MoveLoadingUnitMessageData(
                            this.Mission.MissionType,
                            this.Mission.LoadingUnitSource,
                            this.Mission.LoadingUnitDestination,
                            this.Mission.LoadingUnitCellSourceId,
                            this.Mission.DestinationCellId,
                            this.Mission.LoadingUnitId,
                            (this.Mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                            isEject,
                            this.Mission.FsmId,
                            this.Mission.Action);

                        var msg = new NotificationMessage(
                            messageData,
                            $"Loading Unit {this.Mission.LoadingUnitId} placed on bay {bay.Number}",
                            MessageActor.AutomationService,
                            MessageActor.MachineManager,
                            MessageType.MoveLoadingUnit,
                            notification.RequestingBay,
                            bay.Number,
                            MessageStatus.OperationWaitResume);
                        this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);

                        if (this.Mission.WmsId.HasValue)
                        {
                            if (bay.Positions.Count() == 1
                                || bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadingUnitDestination).IsUpper
                                || bay.Carousel is null)
                            {
                                var newStep = new MissionMoveWaitPickState(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep.OnEnter(null);
                            }
                            else
                            {
                                var newStep = new MissionMoveBayChainState(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep.OnEnter(null);
                            }
                        }
                        else
                        {
                            var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
                    }
                    else
                    {
                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }

                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        #endregion
    }
}
