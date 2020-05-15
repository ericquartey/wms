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
        private const double LoadUnitMaxNetWeightBayChainPercent = 1.1;

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
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.BayChain;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
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
            if (this.Mission.RestoreConditions
                && this.LoadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number)
                && Math.Abs(this.BaysDataProvider.GetChainPosition(bay.Number) - bay.Carousel.LastIdealPosition) > Math.Abs(bay.ChainOffset) + 1
                )
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.AutomaticRestoreNotAllowed, bay.Number);
                throw new StateMachineException(ErrorDescriptions.AutomaticRestoreNotAllowed, bay.Number, MessageActor.MachineManager);
            }
            var position = bay.Positions.FirstOrDefault(p => !p.IsUpper);
            if (position != null
                && position.LoadingUnit != null
                && position.LoadingUnit.Id == this.Mission.LoadUnitId
                && !this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                && this.LoadingUnitMovementProvider.IsOnlyBottomPositionOccupied(bay.Number)
                && (position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare) > (position.LoadingUnit.MaxNetWeight * LoadUnitMaxNetWeightBayChainPercent)
                )
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed, bay.Number);

                this.Mission.ErrorCode = MachineErrorCode.NoError;
                this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);

                var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
                return true;
            }
            try
            {
                if (this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == destination.Location && m.Id != this.Mission.Id))
                {
                    throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, bay.Number, MessageActor.MachineManager);
                }
                this.Mission.Status = MissionStatus.Executing;
                this.LoadingUnitMovementProvider.MoveCarousel(this.Mission.LoadUnitId, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
                this.Mission.LoadUnitDestination = destination.Location;

                var shutterInverter = bay.Shutter.Inverter.Index;
                if (bay.Shutter.Type == ShutterType.ThreeSensors
                    && this.SensorsProvider.GetShutterPosition(shutterInverter) == ShutterPosition.Closed)
                {
                    this.Logger.LogInformation($"Half Shutter Mission:Id={this.Mission.Id}");
                    this.Mission.OpenShutterPosition = ShutterPosition.Half;
                    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                }
            }
            catch (StateMachineException ex)
            {
                var description = $"Move Bay chain not allowed at the moment in bay {bay.Number}. Reason {ex.NotificationMessage.Description}. Wait for resume";
                // we don't want any exception here because this is the normal procedure:
                // send a second LU in lower position while operator is working on upper position
                this.Logger.LogInformation(description);
                this.Mission.Status = MissionStatus.Waiting;
            }

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
                    if (this.Mission.Status == MissionStatus.Executing
                        && (notification.Type == MessageType.ShutterPositioning
                            || notification.RequestingBay == this.Mission.TargetBay
                            )
                        )
                    {
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
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
                                    this.BaysDataProvider.SetLoadingUnit(destination.Id, this.Mission.LoadUnitId);
                                    transaction.Commit();
                                }

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
                                    break;
                                }
                            }
                        }
                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition != this.Mission.OpenShutterPosition)
                            {
                                this.Logger.LogError(ErrorDescriptions.LoadUnitShutterClosed);
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error, !this.ErrorsProvider.IsErrorSmall());
                                break;
                            }
                        }
                        if (notification.Type == MessageType.Homing
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
                    if (this.Mission.Status == MissionStatus.Executing
                        && notification.RequestingBay == this.Mission.TargetBay
                        )
                    {
                        this.OnStop(StopRequestReason.Error);
                    }
                    break;
            }
        }

        private void BayChainEnd()
        {
            if (this.Mission.MissionType == MissionType.OUT
                || this.Mission.MissionType == MissionType.WMS
                || this.Mission.MissionType == MissionType.FullTestOUT
                )
            {
                var waitingMission = this.MissionsDataProvider.GetAllActiveMissions()
                    .FirstOrDefault(m => m.LoadUnitSource == this.Mission.LoadUnitDestination
                        && m.Step == MissionStep.WaitDeposit);

                if (waitingMission != null)
                {
                    // wake up the mission waiting for the bay chain movement
                    this.Logger.LogInformation($"Resume waiting deposit Mission:Id={waitingMission.Id}");
                    this.LoadingUnitMovementProvider.ResumeOperation(
                        waitingMission.Id,
                        waitingMission.LoadUnitSource,
                        waitingMission.LoadUnitDestination,
                        waitingMission.WmsId,
                        waitingMission.MissionType,
                        waitingMission.TargetBay,
                        MessageActor.MachineManager);
                }

                var newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
            else
            {
                if (!this.CheckMissionShowError())
                {
                    this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                }
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
                && !this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == destination.Location && m.Id != this.Mission.Id)
                )
#endif
            {
                var position = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                if (position != null
                    && position.LoadingUnit != null
                    && position.LoadingUnit.Id == this.Mission.LoadUnitId
                    && (position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare) > (position.LoadingUnit.MaxNetWeight * LoadUnitMaxNetWeightBayChainPercent)
                    )
                {
                    this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed, bay.Number);

                    this.Mission.ErrorCode = MachineErrorCode.NoError;
                    this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
                    this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
                    this.BaysDataProvider.Light(this.Mission.TargetBay, true);

                    var newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                    return;
                }
                try
                {
                    this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                    this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                    this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);
                    var shutterInverter = bay.Shutter.Inverter.Index;
                    if (bay.Shutter.Type == ShutterType.ThreeSensors
                        && this.SensorsProvider.GetShutterPosition(shutterInverter) == ShutterPosition.Closed)
                    {
                        this.Logger.LogInformation($"Half Shutter Mission:Id={this.Mission.Id}");
                        this.Mission.OpenShutterPosition = ShutterPosition.Half;
                    }
                    this.Mission.Status = MissionStatus.Executing;

                    this.LoadingUnitMovementProvider.MoveCarousel(this.Mission.LoadUnitId, MessageActor.MachineManager, bay.Number, false);
                    this.Mission.LoadUnitDestination = destination.Location;
                    this.Mission.StepTime = DateTime.UtcNow;
                    this.MissionsDataProvider.Update(this.Mission);
                    this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

                    this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationStart);

                    if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                    {
                        this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                    }
                }
                catch (StateMachineException)
                {
                    //this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed, this.Mission.TargetBay);
                    //throw new StateMachineException(ErrorDescriptions.MoveBayChainNotAllowed, this.Mission.TargetBay, MessageActor.MachineManager);
                    this.Logger.LogInformation($"Move Bay chain not allowed at the moment. Wait for another resume.");
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
