using System;
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
    public class MissionMoveBackToTargetStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveBackToTargetStep(Mission mission,
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

            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.BackToTarget;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.EjectLoadUnit = false;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            if (destinationHeight is null)
            {
                if (this.Mission.LoadUnitDestination == LoadingUnitLocation.Cell)
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationCell, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationCell, this.Mission.TargetBay, MessageActor.MachineManager);
                }
                else
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                    throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
                }
            }
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            var shutterInverter = bay.Shutter.Inverter.Index;
            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition == ShutterPosition.Closed)
            {
                this.Mission.CloseShutterBayNumber = BayNumber.None;
            }
            else
            {
                this.Mission.CloseShutterBayNumber = (bay.Shutter.Type != ShutterType.NotSpecified ? bay.Number : BayNumber.None);
            }

            var waitContinue = (this.Mission.CloseShutterBayNumber != BayNumber.None && !bay.IsExternal);
            this.Logger.LogInformation($"PositionElevatorToPosition start: target {destinationHeight.Value}, closeShutterBay {this.Mission.CloseShutterBayNumber}, closeShutterPosition {this.Mission.CloseShutterPosition}, measure {false}, waitContinue {waitContinue}, Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
            this.LoadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                this.Mission.CloseShutterBayNumber,
                this.Mission.CloseShutterPosition,
                measure: false,
                MessageActor.MachineManager,
                this.Mission.TargetBay,
                this.Mission.RestoreConditions,
                targetBayPositionId,
                targetCellId,
                waitContinue);
            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);
            var measure = (this.Mission.LoadUnitSource != LoadingUnitLocation.Cell);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.Type == MessageType.ShutterPositioning
                        || notification.TargetBay == BayNumber.ElevatorBay
                        )
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                            if (notification.Type == MessageType.ShutterPositioning)
                            {
                                this.Logger.LogDebug($"ContinuePositioning Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
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

            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                && (this.Mission.CloseShutterBayNumber == BayNumber.None
                    || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                )
            {
                var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        #endregion
    }
}
