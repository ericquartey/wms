using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveWaitPickState : MissionMoveBase
    {
        #region Constructors

        public MissionMoveWaitPickState(Mission mission,
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
            this.Mission.FsmStateName = nameof(MissionMoveWaitPickState);
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadingUnitDestination);

            this.LoadingUnitMovementProvider.NotifyAssignedMissionOperationChanged(bay.Number, this.Mission.WmsId.Value);

            if (this.Mission.LoadingUnitId > 0)
            {
                this.LoadingUnitsDataProvider.SetHeight(this.Mission.LoadingUnitId, 0);
            }
            this.Mission.Status = MissionStatus.Waiting;
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
        }

        public override void OnResume(CommandMessage command)
        {
#if CHECK_BAY_SENSOR
            if (!this.sensorsProvider.IsLoadingUnitInLocation(this.ejectBay))
#endif
            {
                if (command != null
                    && command.Data is IMoveLoadingUnitMessageData messageData
                    )
                {
                    var ejectBayLocation = this.Mission.LoadingUnitDestination;
                    var bayPosition = this.BaysDataProvider.GetPositionByLocation(ejectBayLocation);
                    if (messageData.MissionType == MissionType.NoType)
                    {
                        // Remove LoadUnit

                        var lu = bayPosition.LoadingUnit?.Id ?? throw new EntityNotFoundException($"LoadingUnit by BayPosition ID={bayPosition.Id}");

                        this.BaysDataProvider.RemoveLoadingUnit(lu);

                        var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    else
                    {
                        // Update mission and start moving
                        this.Mission.MissionType = messageData.MissionType;
                        this.Mission.WmsId = messageData.WmsId;
                        this.Mission.LoadingUnitSource = bayPosition.Location;
                        if (messageData.Destination == LoadingUnitLocation.Cell)
                        {
                            // prepare for finding a new empty cell
                            this.Mission.DestinationCellId = null;
                            this.Mission.LoadingUnitDestination = LoadingUnitLocation.Cell;
                        }
                        else if (bayPosition.Location != messageData.Destination)
                        {
                            // bay to bay movement
                            this.Mission.LoadingUnitDestination = messageData.Destination;
                        }
                        this.MissionsDataProvider.Update(this.Mission);
                        var newStep = new MissionMoveStartState(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                }
                else
                {
                    var description = $"{this.GetType().Name}:OnResume: Invalid command";
                    throw new StateMachineException(description, this.Mission.TargetBay, MessageActor.MachineManager);
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitNotRemoved, this.requestingBay);
            }
#endif
        }

        #endregion
    }
}
