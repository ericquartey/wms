using System;
using System.Linq;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.Status = MissionStatus.Waiting;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);

            if (this.Mission.WmsId.HasValue)
            {
                this.LoadingUnitMovementProvider.NotifyAssignedMissionOperationChanged(bay.Number, this.Mission.Id);
            }
            else if (!bay.IsDouble
                || (bay.Positions.FirstOrDefault(p => p.Location == this.Mission.LoadUnitDestination)?.LoadingUnit?.Id ?? this.Mission.LoadUnitId) == this.Mission.LoadUnitId
                )
            {
                this.BaysDataProvider.AssignMission(this.Mission.TargetBay, this.Mission);
            }

            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(bay.Number, this.Mission.Step.ToString(), MessageStatus.OperationWaitResume);

            // Set the light ON for the target bay. Handle exceptional case, if exist already a waiting mission in the current internal double bay
            if (bay.IsDouble &&
                bay.Carousel == null &&
                !bay.IsExternal)
            {
                // Handle only for BID
                var waitMissions = this.MissionsDataProvider.GetAllMissions()
                    .Where(
                        m => m.LoadUnitId != this.Mission.LoadUnitId &&
                        m.Id != this.Mission.Id &&
                        m.Status == MissionStatus.Waiting &&
                        m.Step == MissionStep.WaitPick &&
                        bay.Positions.Any(p => p.LoadingUnit?.Id == m.LoadUnitId)
                    );

                if (waitMissions.Any())
                {
                    // There is another waiting mission in the bay, so the light is set to Off
                    this.BaysDataProvider.Light(this.Mission.TargetBay, false);
                    this.Logger.LogDebug($"Light bay {bay.Number} is false");
                }
                else
                {
                    this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                    this.Logger.LogDebug($"Light bay {bay.Number} is true");
                }
            }
            else if (bay.IsRobot
                && (this.Mission.MissionType == MissionType.OUT || this.Mission.MissionType == MissionType.WMS)
                )
            {
                this.BaysDataProvider.EndMissionRobot(bay.Number, true);
            }
            else
            {
                // All others bay configuration
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
            }

            this.BaysDataProvider.CheckIntrusion(this.Mission.TargetBay, true);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            switch (notification.Status)
            {
                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    if ((notification.RequestingBay == this.Mission.TargetBay || notification.RequestingBay == BayNumber.None)
                        && !this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.EjectLoadUnit)
                        )
                    {
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                        if (bay != null && bay.IsRobot)
                        {
                            this.BaysDataProvider.EndMissionRobot(bay.Number, false);
                        }
                        this.OnStop(StopRequestReason.Error);
                    }
                    break;
            }
            switch (notification.Type)
            {
                case MessageType.Stop:
                    if ((notification.RequestingBay == this.Mission.TargetBay
                        || notification.TargetBay == this.Mission.TargetBay
                        )
                        && !this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.EjectLoadUnit)
                        && !this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.ErrorMovements.HasFlag(MissionErrorMovements.AbortMovement))
                        )
                    {
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                        if (bay != null && bay.IsRobot)
                        {
                            this.BaysDataProvider.EndMissionRobot(bay.Number, false);
                        }
                        this.OnStop(StopRequestReason.Error);
                    }
                    break;
            }
        }

        public override void OnResume(CommandMessage command)
        {
            this.Mission.Status = MissionStatus.Executing;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var ejectBayLocation = this.Mission.LoadUnitDestination;
            var bayPosition = this.BaysDataProvider.GetPositionByLocation(ejectBayLocation);
            if (bayPosition.Bay.IsRobot)
            {
                this.BaysDataProvider.EndMissionRobot(bayPosition.Bay.Number, false);
            }
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
                        this.Mission.StepTime = DateTime.UtcNow;
                        // Remove LoadUnit

                        var lu = bayPosition.LoadingUnit?.Id ?? throw new EntityNotFoundException($"LoadingUnit by BayPosition ID={bayPosition.Id}");

                        this.Logger.LogInformation($"Remove load unit {lu} Mission:Id={this.Mission.Id}");
                        this.BaysDataProvider.RemoveLoadingUnit(lu);

                        var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        newStep.OnEnter(null);
                    }
                    else
                    {
                        if (this.Mission.ErrorCode != MachineErrorCode.NoError)
                        {
                            this.Logger.LogInformation($"Mission:Id={this.Mission.Id}, ErrorCode={this.Mission.ErrorCode}. Prompt to display the error condition view...");

                            // Handle error condition for the given mission, if exists
                            //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                            this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(this.Mission.TargetBay);
                            this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
                            this.ErrorsProvider.RecordNew(this.Mission.ErrorCode, this.Mission.TargetBay);
                            this.BaysDataProvider.Light(this.Mission.TargetBay, true);

                            // End (forced) the current mission
                            var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);

                            return;
                        }

                        var activeMission = this.MissionsDataProvider.GetAllActiveMissions()
                            .FirstOrDefault(x => x.Status == MissionStatus.Executing
                                && x.Id != this.Mission.Id
                                );

                        if (activeMission != null)
                        {
                            this.Logger.LogInformation($"{ErrorReasons.AnotherMissionIsActiveForThisBay} Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                            this.Mission.Status = MissionStatus.Waiting;
                            this.MissionsDataProvider.Update(this.Mission);
                        }
                        else
                        {
                            // Update mission and start moving
                            this.Mission.StepTime = DateTime.UtcNow;
                            this.Mission.MissionType = messageData.MissionType;
                            this.Mission.WmsId = messageData.WmsId;
                            this.Mission.LoadUnitSource = bayPosition.Location;
                            if (messageData.Destination == LoadingUnitLocation.Cell
                                || this.Mission.LoadUnitDestination == LoadingUnitLocation.NoLocation
                                )
                            {
                                // prepare for finding a new empty cell
                                this.Mission.DestinationCellId = null;
                                this.Mission.LoadUnitDestination = LoadingUnitLocation.Cell;
                            }
                            else if (bayPosition.Location != messageData.Destination
                                && messageData.Destination != LoadingUnitLocation.NoLocation
                                )
                            {
                                // bay to bay movement
                                this.Mission.LoadUnitDestination = messageData.Destination;
                            }
                            this.LoadingUnitsDataProvider.SetStatus(this.Mission.LoadUnitId, DataModels.Enumerations.LoadingUnitStatus.OnMovementToLocation);

                            this.MissionsDataProvider.Update(this.Mission);
                            var newStep = new MissionMoveStartStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                            newStep.OnEnter(null);
                        }
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
