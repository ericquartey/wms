using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveBayChainStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveBayChainStep(Mission mission,
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
            this.Mission.Step = MissionStep.BayChain;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
            if (destination is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);
#if CHECK_BAY_SENSOR
            if (this.LoadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number))
#endif
            {
                this.Mission.RestoreConditions = false;
            }
            try
            {
                this.LoadingUnitMovementProvider.MoveCarousel(this.Mission.LoadUnitId, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
            }
            catch (StateMachineException ex)
            {
                var description = $"Move Bay chain not possible in bay {bay.Number}. Reason {ex.NotificationMessage.Description}. Wait for resume";
                // we don't want any exception here because this is the normal procedure:
                // send a second LU in lower position while operator is working on upper position
                this.Logger.LogInformation(description);
                this.Mission.Status = MissionStatus.Waiting;
            }

            this.Mission.RestoreConditions = false;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay,
                this.Mission.Step.ToString(),
                (this.Mission.Status == MissionStatus.Waiting) ? MessageStatus.OperationEnd : MessageStatus.OperationExecuting);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.CarouselStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (notification.RequestingBay == this.Mission.TargetBay)
                    {
                        if (notification.Type == MessageType.Positioning
                            && notification.TargetBay == notification.RequestingBay
                            )
                        {
                            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                            if (destination is null)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                            if (origin is null)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedBottom, this.Mission.TargetBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedBottom, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            using (var transaction = this.ElevatorDataProvider.GetContextTransaction())
                            {
                                this.BaysDataProvider.SetLoadingUnit(origin.Id, null);
                                this.BaysDataProvider.SetLoadingUnit(destination.Id, this.Mission.LoadUnitId, 0);
                            }
                            this.Mission.LoadUnitDestination = destination.Location;

                            var notificationText = $"Load Unit {this.Mission.LoadUnitId} placed on bay {bay.Number}";
                            this.SendMoveNotification(bay.Number, notificationText, MessageStatus.OperationWaitResume);

                            if (this.Mission.RestoreConditions)
                            {
                                this.LoadingUnitMovementProvider.UpdateLastBayChainPosition(this.Mission.TargetBay);
                                this.Mission.RestoreConditions = false;
                            }
                            if (this.Mission.NeedHomingAxis == Axis.BayChain
                                && bay.Positions.Count(p => p.LoadingUnit != null) < 2
                                )
                            {
                                this.MissionsDataProvider.Update(this.Mission);
                                this.Logger.LogInformation($"Homing Bay occupied start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.Mission.LoadUnitId, true, notification.RequestingBay, MessageActor.MachineManager);
                            }
                            else
                            {
                                this.BayChainEnd();
                            }
                        }
                        else if (notification.Type == MessageType.Homing
                            && notification.Data is HomingMessageData messageData)
                        {
                            if (messageData.AxisToCalibrate == Axis.Horizontal
                                && !this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator)
                                )
                            {
                                this.MachineVolatileDataProvider.IsHomingExecuted = true;
                            }
                            if (messageData.AxisToCalibrate == Axis.BayChain
                                && this.LoadingUnitMovementProvider.IsOnlyTopPositionOccupied(this.Mission.TargetBay)
                                )
                            {
                                this.BayChainEnd();
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

        private void BayChainEnd()
        {
            if (this.Mission.MissionType == MissionType.OUT
                || this.Mission.MissionType == MissionType.WMS
                )
            {
                var newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        public override void OnResume(CommandMessage command)
        {
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
#if CHECK_BAY_SENSOR
            if (!this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                && this.LoadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number)
                )
#endif
            {
                try
                {
                    this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);
                    this.Mission.Status = MissionStatus.Executing;
                    this.MissionsDataProvider.Update(this.Mission);
                    this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

                    this.LoadingUnitMovementProvider.MoveCarousel(this.Mission.LoadUnitId, MessageActor.MachineManager, bay.Number, false);
                    this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationStart);
                }
                catch (StateMachineException ex)
                {
                    //this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed, this.Mission.TargetBay);
                    //throw new StateMachineException(ErrorDescriptions.MoveBayChainNotAllowed, this.Mission.TargetBay, MessageActor.MachineManager);
                    this.Logger.LogInformation(ErrorDescriptions.MoveBayChainNotAllowed);
                }
            }
#if CHECK_BAY_SENSOR
            else
            {
                //this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotRemoved, this.Mission.TargetBay);
                //throw new StateMachineException(ErrorDescriptions.LoadUnitNotRemoved, this.Mission.TargetBay, MessageActor.MachineManager);
                this.Logger.LogInformation(ErrorDescriptions.LoadUnitNotRemoved);
            }
#endif
        }

        #endregion
    }
}
