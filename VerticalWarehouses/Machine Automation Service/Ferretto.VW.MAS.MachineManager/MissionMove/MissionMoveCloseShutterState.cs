using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
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
            this.Mission.RestoreState = MissionState.NotDefined;
            this.Mission.State = MissionState.CloseShutter;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);

            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            bool isEject = this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.LoadUnit
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.NoLocation;
            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.State.ToString(), isEject, MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.ShutterStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.Homing)
                    {
                        this.Mission.NeedHomingAxis = Axis.None;
                        this.CloseShutterEnd();
                    }
                    else
                    {
                        if (this.Mission.NeedHomingAxis == Axis.Horizontal)
                        {
                            this.Logger.LogDebug($"Homing elevator free start");
                            this.LoadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.Mission.LoadUnitId, notification.RequestingBay, MessageActor.MachineManager);
                        }
                        else
                        {
                            this.CloseShutterEnd();
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        private void CloseShutterEnd()
        {
            bool isEject = this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.LoadUnit
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.NoLocation;
            if (isEject)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                var description = $"Load Unit {this.Mission.LoadUnitId} placed on bay {bay.Number}";
                this.SendMoveNotification(bay.Number, description, isEject, MessageStatus.OperationWaitResume);

                if (this.Mission.WmsId.HasValue)
                {
                    if (bay.Positions.Count() == 1
                        || bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitDestination).IsUpper
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
        }

        #endregion
    }
}
