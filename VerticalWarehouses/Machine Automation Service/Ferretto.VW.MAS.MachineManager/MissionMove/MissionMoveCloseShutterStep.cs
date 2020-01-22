using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
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
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.CloseShutter;
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

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.ShutterStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    this.CloseShutterEnd();
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
            IMissionMoveBase newStep;
            if (this.Mission.LoadUnitDestination != LoadingUnitLocation.Cell
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.Elevator
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.LoadUnit
                && this.Mission.LoadUnitDestination != LoadingUnitLocation.NoLocation)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                var description = $"Load Unit {this.Mission.LoadUnitId} placed on bay {bay.Number}";
                this.SendMoveNotification(bay.Number, description, MessageStatus.OperationWaitResume);

                if (bay.Positions.Count() == 1
                    || bay.Positions.FirstOrDefault(x => x.Location == this.Mission.LoadUnitDestination).IsUpper
                    || bay.Carousel is null)
                {
                    if (this.Mission.WmsId.HasValue)
                    {
                        newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }
                else
                {
                    newStep = new MissionMoveBayChainStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                }
            }
            else
            {
                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            newStep.OnEnter(null);
        }

        #endregion
    }
}
