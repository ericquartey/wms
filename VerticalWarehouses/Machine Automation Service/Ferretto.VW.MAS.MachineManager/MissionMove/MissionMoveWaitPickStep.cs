using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveWaitPickStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveWaitPickStep(Mission mission,
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
            this.Mission.Step = MissionStep.WaitPick;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.Status = MissionStatus.Waiting;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);

            if (this.Mission.WmsId.HasValue)
            {
                this.LoadingUnitMovementProvider.NotifyAssignedMissionOperationChanged(bay.Number, this.Mission.WmsId.Value);
            }
            else if (!bay.IsDouble
                || (bay.Positions.FirstOrDefault(p => p.Location == this.Mission.LoadUnitDestination)?.LoadingUnit.Id ?? this.Mission.LoadUnitId) == this.Mission.LoadUnitId
                )
            {
                this.BaysDataProvider.AssignWmsMission(this.Mission.TargetBay, this.Mission, null);
            }

            if (this.Mission.LoadUnitId > 0)
            {
                this.LoadingUnitsDataProvider.SetHeight(this.Mission.LoadUnitId, 0);
            }
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);

            var description = $"Load Unit {this.Mission.LoadUnitId} placed on bay {bay.Number}";
            this.SendMoveNotification(bay.Number, description, MessageStatus.OperationWaitResume);
            this.BaysDataProvider.Light(this.Mission.TargetBay, true);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
        }

        public override void OnResume(CommandMessage command)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            var ejectBayLocation = this.Mission.LoadUnitDestination;
            var bayPosition = this.BaysDataProvider.GetPositionByLocation(ejectBayLocation);
#if CHECK_BAY_SENSOR
            if (this.SensorsProvider.IsLoadingUnitInLocation(ejectBayLocation))
#endif
            {
                if (command != null
                    && command.Data is IMoveLoadingUnitMessageData messageData
                    )
                {
                    if (messageData.MissionType == MissionType.NoType)
                    {
                        // Remove LoadUnit

                        var lu = bayPosition.LoadingUnit?.Id ?? throw new EntityNotFoundException($"LoadingUnit by BayPosition ID={bayPosition.Id}");

                        this.BaysDataProvider.RemoveLoadingUnit(lu);

                        var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    else
                    {
                        // Update mission and start moving
                        this.Mission.MissionType = messageData.MissionType;
                        this.Mission.WmsId = messageData.WmsId;
                        this.Mission.LoadUnitSource = bayPosition.Location;
                        if (messageData.Destination == LoadingUnitLocation.Cell)
                        {
                            // prepare for finding a new empty cell
                            this.Mission.DestinationCellId = null;
                            this.Mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                        }
                        else if (bayPosition.Location != messageData.Destination)
                        {
                            // bay to bay movement
                            this.Mission.LoadUnitDestination = messageData.Destination;
                        }
                        this.MissionsDataProvider.Update(this.Mission);
                        var newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                }
                else
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.ResumeCommandNotValid, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.ResumeCommandNotValid, this.Mission.TargetBay, MessageActor.MachineManager);
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitMissingOnBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitMissingOnBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }
#endif
        }

        #endregion
    }
}
