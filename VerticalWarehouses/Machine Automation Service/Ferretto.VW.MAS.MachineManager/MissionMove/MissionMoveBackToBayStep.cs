using System;
using System.Linq;
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
    public class MissionMoveBackToBayStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveBackToBayStep(Mission mission,
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
            this.Mission.Step = MissionStep.BackToBay;
            this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
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
            // if destination is busy try the other
            if (bay.Carousel != null
                && !this.DestinationOk(bay, ref destinationHeight, ref targetBayPositionId, ref targetCellId)
                )
            {
                this.Logger.LogInformation($"Destination {this.Mission.LoadUnitDestination} is busy, waiting for resume. Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                this.Mission.Status = MissionStatus.Waiting;
                this.MissionsDataProvider.Update(this.Mission);
                return true;
            }
            this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
            this.MissionsDataProvider.Update(this.Mission);

            this.StartMovement(destinationHeight, targetBayPositionId, targetCellId, bay);
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
                        // Light ON, if a loading unit is waiting into bay for a internal double bay
                        var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);

                        if (this.MachineVolatileDataProvider.IsBayLightOn.ContainsKey(this.Mission.TargetBay) &&
                            bay.IsDouble &&
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
                                if (!this.MachineVolatileDataProvider.IsBayLightOn[this.Mission.TargetBay])
                                {
                                    this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                                }
                            }
                        }

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
                case MessageStatus.OperationFaultStop:
                    this.OnStop(StopRequestReason.Error);
                    break;
            }

            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                && (this.Mission.CloseShutterBayNumber == BayNumber.None
                    || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                )
            {
                // Use a different step according to the current configuration bay
                var isAtLeastOneWaitingMission = this.isWaitingMissionOnThisBay();
                if (!isAtLeastOneWaitingMission)
                {
                    // Eject the loading unit onto the current bay
                    // Use for everything configuration bay types but the internal double bay
                    var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    // Send a notification message to UI

                    // The loading unit is waiting to unload in the bay.
                    // Only applied for the double internal bay
                    var newStep = new MissionMoveWaitDepositBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
            }
        }

        public override void OnResume(CommandMessage command)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            var destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out var targetBayPositionId, out var targetCellId);
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }
            // if destination is busy try the other
            if (bay.Carousel != null
                && !this.DestinationOk(bay, ref destinationHeight, ref targetBayPositionId, ref targetCellId)
                )
            {
                this.Logger.LogInformation($"Destination {this.Mission.LoadUnitDestination} is busy, waiting for next resume. Mission:Id={this.Mission.Id}, Load Unit {this.Mission.LoadUnitId}");
                return;
            }
            this.StartMovement(destinationHeight, targetBayPositionId, targetCellId, bay);
        }

        /// <summary>
        /// check if destination is free.
        /// we can also change destination in some situations.
        /// </summary>
        /// <param name="bay"></param>
        /// <param name="destinationHeight"></param>
        /// <param name="targetBayPositionId"></param>
        /// <param name="targetCellId"></param>
        /// <returns></returns>
        private bool DestinationOk(Bay bay, ref double? destinationHeight, ref int? targetBayPositionId, ref int? targetCellId)
        {
            var ok = true;
            var destination = bay.Positions.FirstOrDefault(p => p.Location == this.Mission.LoadUnitDestination);
            var otherPosition = bay.Positions.FirstOrDefault(p => p.IsUpper == !destination.IsUpper);
            if (destination.IsUpper
                && otherPosition != null
                && !this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                && this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == destination.Location && m.Id != this.Mission.Id && m.Status != MissionStatus.Waiting)
                )
            {
                // destination is upper and baychain is moving: we must wait
                ok = false;
            }
            else if (destination.IsUpper
                && otherPosition != null
                && !this.SensorsProvider.IsLoadingUnitInLocation(otherPosition.Location)
                && this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                )
            {
                // destination is upper, bay chain is free for the previous check, and upper position is busy: we can go to lower position
                this.Mission.LoadUnitDestination = otherPosition.Location;
                destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out targetBayPositionId, out targetCellId);
            }
            else if (!destination.IsUpper
                && otherPosition != null
                && this.SensorsProvider.IsLoadingUnitInLocation(destination.Location)
                )
            {
                // destination is lower and is occupied: we must wait. Upper destination is not available when lower is busy
                ok = false;
            }
            else if (!destination.IsUpper
                && otherPosition != null
                && !this.SensorsProvider.IsLoadingUnitInLocation(otherPosition.Location)
                && this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == otherPosition.Location && m.Id != this.Mission.Id)
                )
            {
                // destination is lower and baychain is moving: we must wait
                ok = false;
            }
            else if (!destination.IsUpper
                && otherPosition != null
                && !this.SensorsProvider.IsLoadingUnitInLocation(otherPosition.Location)
                )
            {
                // destination is lower and upper is free: we can go to upper position
                this.Mission.LoadUnitDestination = otherPosition.Location;
                destinationHeight = this.LoadingUnitMovementProvider.GetDestinationHeight(this.Mission, out targetBayPositionId, out targetCellId);
            }
            return ok;
        }

        /// <summary>
        /// Check if exist at least a waiting mission (step == MissionStep.WaitPick) in the current bay.
        /// Applied only for internal double bay.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if exists at least a waiting mission,
        ///     <c>false</c> otherwise.
        /// </returns>
        private bool isWaitingMissionOnThisBay()
        {
            var retValue = false;

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitDestination);
            if (bay != null)
            {
                // Only applied for the internal double bay
                if (bay.IsDouble && bay.Carousel == null && !bay.IsExternal)
                {
                    // List of waiting mission on the bay
                    var waitMissions = this.MissionsDataProvider.GetAllActiveMissionsByBay(this.Mission.TargetBay)
                        .Where(
                            m => m.LoadUnitId != this.Mission.LoadUnitId &&
                            m.Id != this.Mission.Id &&
                            ((m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick)
                            || (m.Status == MissionStatus.New && bay.Positions.Any(p => p.LoadingUnit?.Id == m.LoadUnitId))
                            )
                        );

                    retValue = waitMissions.Any();
                }
            }

            return retValue;
        }

        private void StartMovement(double? destinationHeight, int? targetBayPositionId, int? targetCellId, Bay bay)
        {
            var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition == ShutterPosition.Closed || shutterPosition == this.Mission.CloseShutterPosition)
            {
                this.Mission.CloseShutterBayNumber = BayNumber.None;
            }
            else
            {
                if (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified)
                {
                    this.Mission.CloseShutterBayNumber = bay.Number;
                }
                else
                {
                    this.Mission.CloseShutterBayNumber = BayNumber.None;
                }
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
                this.Mission.LoadUnitId,
                targetBayPositionId,
                targetCellId,
                waitContinue);
            this.Mission.RestoreConditions = false;
            this.Mission.Status = MissionStatus.Executing;
            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay, this.Mission.Step.ToString(), MessageStatus.OperationExecuting);
        }

        #endregion
    }
}
