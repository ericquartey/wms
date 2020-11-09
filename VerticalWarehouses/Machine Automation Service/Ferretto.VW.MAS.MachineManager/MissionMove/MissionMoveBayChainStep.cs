using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveBayChainStep : MissionMoveBase
    {
        private const double LoadUnitMaxNetWeightBayChain = 80;

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
            if (this.Mission.NeedHomingAxis == Axis.None)
            {
                if (bay.TotalCycles - bay.LastCalibrationCycles >= bay.CyclesToCalibrate)
                {
                    this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;
                }
                this.Logger.LogTrace($"NeedHomingAxis{this.Mission.NeedHomingAxis}. CyclesToCalibrate {bay.CyclesToCalibrate}. LastCalibrationCycles {bay.LastCalibrationCycles}. Total cycles {bay.TotalCycles}. Mission:Id={this.Mission.Id}");
            }

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
                && (position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare) > (position.LoadingUnit.MaxNetWeight + LoadUnitMaxNetWeightBayChain)
                )
            {
                this.Mission.ErrorCode = MachineErrorCode.MoveBayChainNotAllowed;
            }
            try
            {
                if (this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == destination.Location && m.Id != this.Mission.Id))
                {
                    throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, bay.Number, MessageActor.MachineManager);
                }
                if (this.Mission.ErrorCode != MachineErrorCode.MoveBayChainNotAllowed)
                {
                    this.LoadingUnitMovementProvider.MoveCarousel(this.Mission.LoadUnitId, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions);
                    this.Mission.LoadUnitDestination = destination.Location;
                }

                var shutterInverter = (bay.Shutter != null) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                if (bay.Shutter != null
                    && bay.Shutter.Type == ShutterType.ThreeSensors
                    && this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Half)
                {
                    this.Logger.LogInformation($"Half Shutter Mission:Id={this.Mission.Id}");
                    this.Mission.OpenShutterPosition = ShutterPosition.Half;
                    this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                }
                else if (this.Mission.ErrorCode == MachineErrorCode.MoveBayChainNotAllowed)
                {
                    this.SetErrorMoveBayChain(bay, position);
                    return true;
                }
                this.Mission.Status = MissionStatus.Executing;
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
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                        if (this.UpdateResponseList(notification.Type))
                        {
                            this.MissionsDataProvider.Update(this.Mission);
                            if (notification.Type == MessageType.Positioning
                                && notification.TargetBay == notification.RequestingBay
                            )
                            {
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
                                    this.BaysDataProvider.IncrementCycles(bay.Number);
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
                            }
                        }
                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (shutterPosition != this.Mission.OpenShutterPosition)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);
                                throw new StateMachineException(ErrorDescriptions.LoadUnitShutterClosed, this.Mission.TargetBay, MessageActor.MachineManager);
                            }
                            if (this.Mission.ErrorCode == MachineErrorCode.MoveBayChainNotAllowed)
                            {
                                var position = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                                this.SetErrorMoveBayChain(bay, position);
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
                case MessageStatus.OperationFaultStop:
                    if (this.Mission.Status == MissionStatus.Executing
                        && notification.RequestingBay == this.Mission.TargetBay
                        )
                    {
                        this.OnStop(StopRequestReason.Error);
                        return;
                    }
                    break;
            }
            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                && (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified
                    || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                )
            {
                var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
                if (this.Mission.NeedHomingAxis == Axis.BayChain
                    && bay.Positions.Count(p => p.LoadingUnit != null) < 2
                    && !this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Homing)
                    )
                {
                    this.Logger.LogDebug($"Waiting for homing Mission:Id={this.Mission.Id}");
                }
                else if (this.Mission.ErrorCode == MachineErrorCode.MoveBayChainNotAllowed)
                {
                    this.SetErrorMoveBayChain(bay, bay.Positions.FirstOrDefault(p => !p.IsUpper));
                }
                else
                {
                    this.BayChainEnd();
                }
            }
        }

        private void BayChainEnd()
        {
            IMissionMoveBase newStep;
            if (this.CheckMissionShowError())
            {
                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                this.BaysDataProvider.CheckIntrusion(this.Mission.TargetBay, true);
                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            else if (this.Mission.MissionType == MissionType.OUT
                || this.Mission.MissionType == MissionType.WMS
                || this.Mission.MissionType == MissionType.FullTestOUT
                )
            {
                var waitingMission = this.MissionsDataProvider.GetAllActiveMissions()
                    .FirstOrDefault(m => m.LoadUnitSource == this.Mission.LoadUnitDestination
                        && (m.Step == MissionStep.WaitDepositCell || m.Step == MissionStep.WaitChain));

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

                newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            else
            {
                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            newStep.OnEnter(null);
        }

        public override void OnResume(CommandMessage command)
        {
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
#if CHECK_BAY_SENSOR
            var machineResourcesProvider = this.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();

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
                    && (position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare) > (position.LoadingUnit.MaxNetWeight + LoadUnitMaxNetWeightBayChain)
                    )
                {
                    this.Mission.ErrorCode = MachineErrorCode.MoveBayChainNotAllowed;
                }
                try
                {
                    this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                    this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                    this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);

                    if (this.Mission.ErrorCode != MachineErrorCode.MoveBayChainNotAllowed)
                    {
                        this.LoadingUnitMovementProvider.MoveCarousel(this.Mission.LoadUnitId, MessageActor.MachineManager, bay.Number, false);
                        this.Mission.LoadUnitDestination = destination.Location;
                        this.Mission.StepTime = DateTime.UtcNow;
                    }
                    var shutterInverter = (bay.Shutter != null) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                    if (bay.Shutter != null
                        && bay.Shutter.Type == ShutterType.ThreeSensors
                        && this.SensorsProvider.GetShutterPosition(shutterInverter) != ShutterPosition.Half)
                    {
                        this.Logger.LogInformation($"Half Shutter Mission:Id={this.Mission.Id}");
                        this.Mission.OpenShutterPosition = ShutterPosition.Half;
                    }

                    if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                    {
                        this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                    }
                    else if (this.Mission.ErrorCode == MachineErrorCode.MoveBayChainNotAllowed)
                    {
                        this.SetErrorMoveBayChain(bay, position);
                        return;
                    }
                    this.Mission.Status = MissionStatus.Executing;
                    this.MissionsDataProvider.Update(this.Mission);
                    this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
                    this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationStart);
                }
                catch (StateMachineException)
                {
                    //this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed, this.Mission.TargetBay);
                    //throw new StateMachineException(ErrorDescriptions.MoveBayChainNotAllowed, this.Mission.TargetBay, MessageActor.MachineManager);
                    this.Logger.LogInformation($"Move Bay chain not allowed at the moment. Wait for another resume.");
                }
            }
#if CHECK_BAY_SENSOR
            else if (!machineResourcesProvider.IsDrawerInBayBottom(bay.Number))
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.BottomLevelBayEmpty, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.BottomLevelBayEmpty, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            else
            {
                //this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitNotRemoved, this.Mission.TargetBay);
                //throw new StateMachineException(ErrorDescriptions.LoadUnitNotRemoved, this.Mission.TargetBay, MessageActor.MachineManager);
                this.Logger.LogInformation(ErrorDescriptions.LoadUnitNotRemoved);
            }
#endif
        }

        private void SetErrorMoveBayChain(Bay bay, BayPosition position)
        {
            this.ErrorsProvider.RecordNew(MachineErrorCode.MoveBayChainNotAllowed,
                bay.Number,
                string.Format(Resources.Missions.RemoveMaterialFromLoadUnit,
                    Math.Round((position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare) - (position.LoadingUnit.MaxNetWeight + LoadUnitMaxNetWeightBayChain)),
                    this.Mission.LoadUnitId,
                    Math.Round(position.LoadingUnit.GrossWeight - position.LoadingUnit.Tare)));

            this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
            this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
            this.BaysDataProvider.Light(this.Mission.TargetBay, true);
            this.BaysDataProvider.CheckIntrusion(this.Mission.TargetBay, true);

            // set gross weight to the maximum that do not show this error again
            this.LoadingUnitsDataProvider.SetWeight(this.Mission.LoadUnitId, position.LoadingUnit.MaxNetWeight + position.LoadingUnit.Tare + this.ElevatorDataProvider.GetWeight());

            this.Mission.ErrorCode = MachineErrorCode.LoadUnitWeightExceeded;
            this.Mission.RestoreStep = this.Mission.Step;
            var newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        #endregion
    }
}
