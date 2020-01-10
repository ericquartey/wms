using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveCloseShutterState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveCloseShutterState(Mission mission,
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
            this.Mission.FsmStateName = nameof(MissionMoveCloseShutterState);
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
            if (bay is null)
            {
                var description = $"{this.GetType().Name}: destination bay not found {this.Mission.LoadingUnitDestination}";

                throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);

            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.ShutterStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    bool isEject = this.Mission.LoadingUnitDestination != LoadingUnitLocation.Cell
                        && this.Mission.LoadingUnitDestination != LoadingUnitLocation.Elevator
                        && this.Mission.LoadingUnitDestination != LoadingUnitLocation.LoadingUnit
                        && this.Mission.LoadingUnitDestination != LoadingUnitLocation.NoLocation;
                    if (isEject)
                    {
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);
                        var description = $"Load Unit {this.Mission.LoadingUnitId} placed on bay {bay.Number}";
                        this.SendMoveNotification(bay.Number, description, isEject, MessageStatus.OperationWaitResume);

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
