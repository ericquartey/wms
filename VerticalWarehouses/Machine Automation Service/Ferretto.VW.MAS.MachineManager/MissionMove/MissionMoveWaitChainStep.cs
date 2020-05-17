using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
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
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.EjectLoadUnit = false;
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.WaitChain;
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
                this.Mission.CloseShutterBayNumber = (bay.Shutter.Type != ShutterType.NotSpecified ? bay.Number : BayNumber.None);
                if (this.Mission.CloseShutterBayNumber != BayNumber.None)
                {
                    this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitSource);
                    var shutterInverter = bay.Shutter.Inverter.Index;
                    if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                    {
                        this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                        this.Mission.CloseShutterBayNumber = BayNumber.None;
                    }
                }
                var waitContinue = (this.Mission.CloseShutterBayNumber != BayNumber.None && !bay.IsExternal);
                LoadingUnit lowerUnit = null;
                if (bay.Carousel != null
                    && (lowerUnit = bay.Positions.FirstOrDefault(p => !p.IsUpper && p.LoadingUnit != null)?.LoadingUnit) != null
                    )
                {
                    var lowerMission = this.MissionsDataProvider.GetAllActiveMissions().FirstOrDefault(m => m.LoadUnitId == lowerUnit.Id);
                    if (lowerMission != null)
                    {
                        this.Logger.LogInformation($"Resume lower bay Mission:Id={lowerMission.Id}");
                        if (waitContinue)
                        {
                            this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.CloseShutterBayNumber, this.Mission.RestoreConditions, this.Mission.CloseShutterPosition);
                        }
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
                        return true;
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
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.ShutterPositioning
                            || notification.RequestingBay == this.Mission.TargetBay)
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            if (notification.Type == MessageType.ShutterPositioning)
                            {
                                this.Mission.CloseShutterBayNumber = BayNumber.None;
                            }
                            this.MissionsDataProvider.Update(this.Mission);
                            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                                && this.Mission.CloseShutterBayNumber == BayNumber.None
                                )
                            {
                                var newStep = new MissionMoveToTargetStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                                newStep.OnEnter(null);
                            }
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

        #endregion
    }
}
