using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveWaitChainStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveWaitChainStep(Mission mission,
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
            this.Mission.EjectLoadUnit = false;
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.WaitChain;
            this.Mission.MissionTime = this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator);
            if (measure)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (bay is null)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                var loadUnitInBay = bay.Positions.FirstOrDefault(p => p.Location == this.Mission.LoadUnitSource)?.LoadingUnit;
                if (this.SensorsProvider.IsLoadingUnitInLocation(this.Mission.LoadUnitSource)
                    && (loadUnitInBay is null
                        || loadUnitInBay.Id == this.Mission.LoadUnitId
                        )
                    )
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotRemoved, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitNotRemoved, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                LoadingUnit lowerUnit = null;
                if (bay.Carousel != null
                    && (lowerUnit = bay.Positions.FirstOrDefault(p => !p.IsUpper && p.LoadingUnit != null)?.LoadingUnit) != null
                    )
                {
                    var lowerMission = this.MissionsDataProvider.GetAllActiveMissions().FirstOrDefault(m => m.LoadUnitId == lowerUnit.Id
                        && m.MissionType != MissionType.IN
                        && m.TargetBay == bay.Number
                        && m.Step > MissionStep.New);
                    if (lowerMission != null)
                    {
                        if (lowerMission.Status == MissionStatus.Waiting)
                        {
                            this.Logger.LogInformation($"Resume lower bay Mission:Id={lowerMission.Id}");
                            this.LoadingUnitMovementProvider.ResumeOperation(
                                lowerMission.Id,
                                lowerMission.LoadUnitSource,
                                lowerMission.LoadUnitDestination,
                                lowerMission.WmsId,
                                lowerMission.MissionType,
                                lowerMission.TargetBay,
                                MessageActor.MachineManager);

                            this.MissionsDataProvider.Update(this.Mission);

                            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
                        }
                        else
                        {
                            this.Logger.LogInformation($"Waiting for resume of lower bay Mission:Id={lowerMission.Id}");
                        }
                        return true;
                    }
                }
                else if (this.Mission.NeedHomingAxis == Axis.None
                    && this.MachineVolatileDataProvider.RandomCells
                    && this.MachineVolatileDataProvider.LoadUnitsToTest?.Count == 1
                    && bay.IsDouble
                    && bay.IsExternal)
                {
                    if (this.Mission.LoadUnitSource == LoadingUnitLocation.InternalBay1Down || this.Mission.LoadUnitSource == LoadingUnitLocation.InternalBay2Down || this.Mission.LoadUnitSource == LoadingUnitLocation.InternalBay3Down)
                    {
                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(null, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, false);
                    }
                    else
                    {
                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(null, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, false);
                    }
                }
                else
                {
                    // Check if exist error mission and resume it
                    var errorMission = this.MissionsDataProvider.GetAllActiveMissions().FirstOrDefault(
                        m => m.ErrorCode != MachineErrorCode.NoError);

                    if (errorMission != null)
                    {
                        var b = this.BaysDataProvider.GetByLoadingUnitLocation(errorMission.LoadUnitDestination);
                        if (b != null)
                        {
                            // Only applied for internal double bay
                            if (b.IsDouble && b.Carousel == null && !b.IsExternal)
                            {
                                this.Logger.LogInformation($"Current mission:Id={this.Mission.Id}, Resume error Mission:Id={errorMission.Id}, ErrorCode={errorMission.ErrorCode}");

                                this.LoadingUnitMovementProvider.ResumeOperation(
                                    errorMission.Id,
                                    errorMission.LoadUnitSource,
                                    errorMission.LoadUnitDestination,
                                    errorMission.WmsId,
                                    errorMission.MissionType,
                                    errorMission.TargetBay,
                                    MessageActor.MachineManager);

                                this.MissionsDataProvider.Update(this.Mission);

                                this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
                            }
                        }
                    }
                }
            }
            // no need to wait
            var newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    // this step do not increase mission time
                    this.Mission.StepTime = DateTime.UtcNow;
                    this.MissionsDataProvider.Update(this.Mission);
                    this.OnStop(StopRequestReason.Error);
                    break;
            }
        }

        public override void OnResume(CommandMessage command)
        {
            // this step do not increase mission time
            this.Mission.StepTime = DateTime.UtcNow;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell && this.Mission.LoadUnitSource != LoadingUnitLocation.Elevator);
            if (measure)
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
                if (bay is null)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                if (bay.Carousel != null
                    && bay.Positions.FirstOrDefault(p => !p.IsUpper && p.LoadingUnit != null)?.LoadingUnit != null
                    )
                {
                    this.Logger.LogInformation($"Waiting for resume of lower bay Mission");
                    return;
                }
            }
            // no need to wait
            var newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        #endregion
    }
}
