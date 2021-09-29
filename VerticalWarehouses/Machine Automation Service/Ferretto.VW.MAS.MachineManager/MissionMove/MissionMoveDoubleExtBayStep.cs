using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
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
    public class MissionMoveDoubleExtBayStep : MissionMoveBase
    {
        #region Fields

        private readonly IMachineResourcesProvider machineResourcesProvider;

        #endregion

        #region Constructors

        public MissionMoveDoubleExtBayStep(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.machineResourcesProvider = this.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.DoubleExtBay;
            this.Mission.MissionTime.Add(DateTime.UtcNow - this.Mission.StepTime);
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }

            var destination = this.SelectPosition(bay);
            //var destination = bay.Positions.FirstOrDefault();

            if (this.MissionsDataProvider.GetAllActiveMissions().Any(m => m.LoadUnitDestination == destination.Location && m.Id != this.Mission.Id))
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitDestinationBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitDestinationBay, bay.Number, MessageActor.MachineManager);
            }

            var isLoadUnitDestinationInBay = (destination.Location == this.Mission.LoadUnitDestination);
            var isLoadingUnitInExternalUpPosition = this.machineResourcesProvider.IsDrawerInBayTop(bay.Number);
            var isLoadingUnitInExternalDownPosition = this.machineResourcesProvider.IsDrawerInBayBottom(bay.Number);
            var isLoadingUnitInInternalUpPosition = this.machineResourcesProvider.IsDrawerInBayInternalTop(bay.Number);
            var isLoadingUnitInInternalDownPosition = this.machineResourcesProvider.IsDrawerInBayInternalBottom(bay.Number);

            // Detect if homing operation is required
            //this.Mission.NeedHomingAxis = (this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] ? Axis.None : Axis.BayChain);

            if (this.Mission.RestoreConditions)
            {
                this.Mission.ErrorCode = MachineErrorCode.NoError;
                if (this.Mission.NeedHomingAxis != Axis.HorizontalAndVertical)
                {
                    this.Mission.NeedHomingAxis = Axis.BayChain;
                }
                this.Logger.LogInformation($"Homing axis {this.Mission.NeedHomingAxis} Start Mission:Id={this.Mission.Id}");
                this.LoadingUnitMovementProvider.Homing(
                    this.Mission.NeedHomingAxis,
                    Calibration.FindSensor,
                    this.Mission.LoadUnitId,
                    true,
                    false,
                    bay.Number,
                    MessageActor.MachineManager);
            }
            else
            {
                // Move during normal positioning
                if (isLoadUnitDestinationInBay)
                {
                    if (destination.IsUpper)
                    {
                        if (isLoadingUnitInInternalUpPosition && !isLoadingUnitInExternalUpPosition)
                        {
                            if (!this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper))
                            {
                                this.ExternalBayChainEnd();
                                return true;
                            }

                            this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                            var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                            if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                            {
                                this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                            }
                            if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified)
                            {
                                this.Logger.LogInformation($"CloseShutter start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                            }
                        }
                        else
                        {
                            this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
                        }
                    }
                    else
                    {
                        if (isLoadingUnitInInternalDownPosition && !isLoadingUnitInExternalDownPosition)
                        {
                            if (!this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper))
                            {
                                this.ExternalBayChainEnd();
                                return true;
                            }

                            this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                            var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                            if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                            {
                                this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                            }
                            if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified)
                            {
                                this.Logger.LogInformation($"CloseShutter start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                            }
                        }
                        else
                        {
                            this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
                        }
                    }
                }
                else
                {
                    if (destination.IsUpper)
                    {
                        if (isLoadingUnitInExternalUpPosition && !isLoadingUnitInInternalUpPosition)
                        {
                            if (!this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper))
                            {
                                this.ExternalBayChainEnd();
                                return true;
                            }

                            this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitSource);
                            var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                            if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                            {
                                this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                            }
                            if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                            {
                                this.Logger.LogInformation($"OpenShutter start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                            }
                        }
                        else
                        {
                            this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
                        }
                    }
                    else
                    {
                        if (isLoadingUnitInExternalDownPosition && !isLoadingUnitInInternalDownPosition)
                        {
                            if (!this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper))
                            {
                                this.ExternalBayChainEnd();
                                return true;
                            }

                            this.Mission.OpenShutterPosition = this.LoadingUnitMovementProvider.GetShutterOpenPosition(bay, this.Mission.LoadUnitSource);
                            var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                            if (this.Mission.OpenShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                            {
                                this.Mission.OpenShutterPosition = ShutterPosition.NotSpecified;
                            }
                            if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified)
                            {
                                this.Logger.LogInformation($"OpenShutter start Mission:Id={this.Mission.Id}");
                                this.LoadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.Mission.OpenShutterPosition, this.Mission.TargetBay, false);
                            }
                        }
                        else
                        {
                            this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
                        }
                    }
                }
            }

            if (this.Mission.ErrorCode == MachineErrorCode.MoveExtBayNotAllowed)
            {
                this.SetErrorMoveExtBayChain(bay, destination);
                return true;
            }
            this.Mission.Status = MissionStatus.Executing;

            this.MissionsDataProvider.Update(this.Mission);

            this.SendMoveNotification(this.Mission.TargetBay,
                this.Mission.Step.ToString(),
                (this.Mission.Status == MissionStatus.Waiting) ? MessageStatus.OperationEnd : MessageStatus.OperationExecuting);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            var notificationStatus = this.LoadingUnitMovementProvider.ExternalBayStatus(notification);

            var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);

            var destination = this.SelectPosition(bay);
            //var destination = bay.Positions.FirstOrDefault();
            var isLoadUnitDestinationInBay = (destination.Location == this.Mission.LoadUnitDestination);

            var isLoadingUnitInExternalUpPosition = this.machineResourcesProvider.IsDrawerInBayTop(bay.Number);
            var isLoadingUnitInExternalDownPosition = this.machineResourcesProvider.IsDrawerInBayBottom(bay.Number);
            var isLoadingUnitInInternalUpPosition = this.machineResourcesProvider.IsDrawerInBayInternalTop(bay.Number);
            var isLoadingUnitInInternalDownPosition = this.machineResourcesProvider.IsDrawerInBayInternalBottom(bay.Number);

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

                            if (notification.Type == MessageType.Positioning &&
                                notification.TargetBay == notification.RequestingBay)
                            {
                                if (destination is null)
                                {
                                    this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitUndefinedUpper, this.Mission.TargetBay);
                                    throw new StateMachineException(ErrorDescriptions.LoadUnitUndefinedUpper, this.Mission.TargetBay, MessageActor.MachineManager);
                                }

                                //if (this.Mission.RestoreConditions)
                                //{
                                //    this.LoadingUnitMovementProvider.UpdateLastBayChainPosition(this.Mission.TargetBay);
                                //    this.Mission.RestoreConditions = false;

                                //    this.Logger.LogDebug($"Restore conditions: {this.Mission.RestoreConditions}");
                                //}
                            }
                        }

                        // Handle the operation end homing operation
                        if (notification.Type == MessageType.Homing &&
                            notification.Data is HomingMessageData messageData)
                        {
                            // Calibrate elevator
                            if (messageData.AxisToCalibrate == Axis.Horizontal || messageData.AxisToCalibrate == Axis.HorizontalAndVertical)
                            {
                                if (!this.SensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator))
                                {
                                    this.MachineVolatileDataProvider.IsHomingExecuted = true;
                                }
                                if (this.Mission.RestoreConditions)
                                {
                                    this.Mission.RestoreConditions = false;
                                    this.Mission.NeedHomingAxis = Axis.BayChain;
                                    this.MissionsDataProvider.Update(this.Mission);

                                    this.Logger.LogInformation($"Homing axis {this.Mission.NeedHomingAxis} Start Mission:Id={this.Mission.Id}");
                                    this.LoadingUnitMovementProvider.Homing(
                                        this.Mission.NeedHomingAxis,
                                        Calibration.FindSensor,
                                        this.Mission.LoadUnitId,
                                        true,
                                        false,
                                        bay.Number,
                                        MessageActor.MachineManager);
                                    return;
                                }
                            }

                            // Calibrate double external bay
                            if (messageData.AxisToCalibrate == Axis.BayChain)
                            {
                                this.Logger.LogDebug($"Homing full BayChain executed: prepare for empty homing");

                                this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;

                                if (isLoadUnitDestinationInBay)
                                {
                                    if ((!destination.IsUpper && isLoadingUnitInExternalDownPosition) ||
                                        (destination.IsUpper && isLoadingUnitInExternalUpPosition))
                                    {
                                        this.ExternalBayChainEnd();
                                        return;
                                    }

                                    if (destination.IsUpper)
                                    {
                                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper);
                                    }
                                    else
                                    {
                                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper);
                                    }
                                    this.Mission.LoadUnitDestination = destination.Location;

                                    this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                                    var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                                    if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                                    {
                                        this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                                    }
                                    if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified)
                                    {
                                        this.Logger.LogInformation($"CloseShutter start Mission:Id={this.Mission.Id}");
                                        this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                                    }

                                    this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                                    this.MissionsDataProvider.Update(this.Mission);
                                    this.Logger.LogDebug($"Execute the move external bay toward Operator, DeviceNotification: {this.Mission.DeviceNotifications}");
                                }
                                else
                                {
                                    this.Logger.LogDebug($"Simulate positioning end");

                                    this.UpdateResponseList(MessageType.Positioning);
                                    this.MissionsDataProvider.Update(this.Mission);
                                }
                            }
                        }

                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterInverter = this.BaysDataProvider.GetShutterInverterIndex(notification.RequestingBay);
                            var shutterPosition = this.SensorsProvider.GetShutterPosition(shutterInverter);
                            if (this.Mission.OpenShutterPosition != ShutterPosition.NotSpecified
                                && shutterPosition != this.Mission.OpenShutterPosition)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterClosed, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error);
                                break;
                            }
                            if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified
                                && shutterPosition != this.Mission.CloseShutterPosition)
                            {
                                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitShutterOpen, notification.RequestingBay);

                                this.OnStop(StopRequestReason.Error);
                                break;
                            }
                        }
                    }
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    if (this.Mission.Status == MissionStatus.Executing
                        && (notification.RequestingBay == this.Mission.TargetBay || notification.RequestingBay == BayNumber.None)
                        )
                    {
                        this.OnStop(StopRequestReason.Error);
                        return;
                    }
                    break;
            }

            if (this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Positioning)
                && (
                    (this.Mission.OpenShutterPosition == ShutterPosition.NotSpecified && this.Mission.CloseShutterPosition == ShutterPosition.NotSpecified)
                        || this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Shutter))
                    )
            {
                //if (this.Mission.NeedHomingAxis == Axis.BayChain &&
                //    !this.Mission.DeviceNotifications.HasFlag(MissionDeviceNotifications.Homing))
                //{
                //    this.Logger.LogDebug($"Waiting for homing Mission:Id={this.Mission.Id}");
                //}
                //else
                if (this.Mission.ErrorCode == MachineErrorCode.MoveExtBayNotAllowed)
                {
                    this.SetErrorMoveExtBayChain(bay, destination);
                    //this.SetErrorMoveExtBayChain(bay, bay.Positions.FirstOrDefault());
                }
                else
                {
                    if (isLoadUnitDestinationInBay && destination.IsUpper && isLoadingUnitInInternalUpPosition && !isLoadingUnitInExternalUpPosition)
                    {
                        this.Logger.LogDebug($"4a. Move double external bay toward Operator, mainly to handle the drawer after a restore conditions movement");

                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper);
                        //this.Mission.LoadUnitDestination = destination.Location;

                        this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    }
                    else if (isLoadUnitDestinationInBay && !destination.IsUpper && isLoadingUnitInInternalDownPosition && !isLoadingUnitInExternalDownPosition)
                    {
                        this.Logger.LogDebug($"4a. Move double external bay toward Operator, mainly to handle the drawer after a restore conditions movement");

                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper);
                        //this.Mission.LoadUnitDestination = destination.Location;

                        this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    }
                    else if (!isLoadUnitDestinationInBay && destination.IsUpper && isLoadingUnitInExternalUpPosition && !isLoadingUnitInInternalUpPosition)
                    {
                        this.Logger.LogDebug($"5a. Move double external bay toward Machine, mainly to handle the drawer after a restore conditions movement");

                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardOperator, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper);
                        //this.Mission.LoadUnitDestination = destination.Location;

                        this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    }
                    else if (!isLoadUnitDestinationInBay && !destination.IsUpper && isLoadingUnitInExternalDownPosition && !isLoadingUnitInInternalDownPosition)
                    {
                        this.Logger.LogDebug($"5a. Move double external bay toward Machine, mainly to handle the drawer after a restore conditions movement");

                        this.LoadingUnitMovementProvider.MoveDoubleExternalBay(this.Mission.LoadUnitId, ExternalBayMovementDirection.TowardMachine, MessageActor.MachineManager, bay.Number, this.Mission.RestoreConditions, destination.IsUpper);
                        //this.Mission.LoadUnitDestination = destination.Location;

                        this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
                    }
                    else
                    {
                        this.Logger.LogDebug($"4. Call the double ExternalBayChainEnd, DeviceNotification: {this.Mission.DeviceNotifications}");
                        this.ExternalBayChainEnd();
                    }
                }
            }
        }

        private void ExternalBayChainEnd()
        {
            IMissionMoveBase newStep;
            if (this.CheckMissionShowError())
            {
                this.WakeUpMission();

                this.Logger.LogDebug($"Detect an error :: go to MissionMoveEndStep");

                this.BaysDataProvider.Light(this.Mission.TargetBay, true);
                newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            }
            else
            {
                var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);

                var destination = this.SelectPosition(bay);
                //var destination = bay.Positions.FirstOrDefault();
                var isLoadUnitDestinationInBay = (destination.Location == this.Mission.LoadUnitDestination);

                var isLoadingUnitInExternalUpPosition = this.machineResourcesProvider.IsDrawerInBayTop(bay.Number);
                var isLoadingUnitInExternalDownPosition = this.machineResourcesProvider.IsDrawerInBayBottom(bay.Number);
                var isLoadingUnitInInternalUpPosition = this.machineResourcesProvider.IsDrawerInBayInternalTop(bay.Number);
                var isLoadingUnitInInternalDownPosition = this.machineResourcesProvider.IsDrawerInBayInternalBottom(bay.Number);

                if (isLoadUnitDestinationInBay)
                {
                    if ((destination.IsUpper && isLoadingUnitInExternalUpPosition && !isLoadingUnitInInternalUpPosition) ||
                        (!destination.IsUpper && isLoadingUnitInExternalDownPosition && !isLoadingUnitInInternalDownPosition))
                    {
                        this.Mission.CloseShutterPosition = this.LoadingUnitMovementProvider.GetShutterClosedPosition(bay, this.Mission.LoadUnitDestination);
                        var shutterInverter = (bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified) ? bay.Shutter.Inverter.Index : InverterDriver.Contracts.InverterIndex.None;
                        if (this.Mission.CloseShutterPosition == this.SensorsProvider.GetShutterPosition(shutterInverter))
                        {
                            this.Mission.CloseShutterPosition = ShutterPosition.NotSpecified;
                        }
                        if (this.Mission.CloseShutterPosition != ShutterPosition.NotSpecified)
                        {
                            this.Logger.LogInformation($"CloseShutter start Mission:Id={this.Mission.Id}");
                            this.LoadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, this.Mission.TargetBay, false, this.Mission.CloseShutterPosition);
                        }

                        //this.Logger.LogDebug($"1. Go to MissionMoveEndStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");
                        if (this.Mission.MissionType == MissionType.OUT ||
                            this.Mission.MissionType == MissionType.WMS ||
                            this.Mission.MissionType == MissionType.FullTestOUT)
                        {
                            newStep = new MissionMoveWaitPickStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                        else
                        {
                            newStep = new MissionMoveEndStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                        }
                    }
                    else
                    {
                        // Detect an error condition
                        //this.Logger.LogDebug($"2. Go to MissionMoveErrorStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");

                        this.ErrorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, this.Mission.TargetBay);
                        this.Mission.RestoreStep = this.Mission.Step;
                        newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                }
                else
                {
                    if ((destination.IsUpper && isLoadingUnitInInternalUpPosition && !isLoadingUnitInExternalUpPosition && this.machineResourcesProvider.IsSensorZeroTopOnBay(bay.Number)) ||
                        (!destination.IsUpper && isLoadingUnitInInternalDownPosition && !isLoadingUnitInExternalDownPosition && this.machineResourcesProvider.IsSensorZeroOnBay(bay.Number)))
                    {
                        //this.Logger.LogDebug($"3. Go to MissionMoveLoadElevatorStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");
                        this.BaysDataProvider.IncrementCycles(bay.Number);
                        if (this.Mission.NeedHomingAxis == Axis.None
                            && bay.TotalCycles - bay.LastCalibrationCycles >= bay.CyclesToCalibrate
                            )
                        {
                            this.MachineVolatileDataProvider.IsBayHomingExecuted[bay.Number] = false;
                        }
                        // resume mission waiting in other position
                        var waitingMission = this.MissionsDataProvider.GetAllActiveMissions()
                            .FirstOrDefault(m => m.LoadUnitSource == this.Mission.LoadUnitDestination
                                && (m.Step == MissionStep.WaitDepositExternalBay));

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

                        newStep = new MissionMoveLoadElevatorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                    }
                    else
                    {
                        // Detect an error condition
                        //this.Logger.LogDebug($"4. Go to MissionMoveErrorStep, IsInternalPositionOccupied: {this.LoadingUnitMovementProvider.IsInternalPositionOccupied(bay.Number)}, IsExternalPositionOccupied: {this.LoadingUnitMovementProvider.IsExternalPositionOccupied(bay.Number)}");

                        this.ErrorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, this.Mission.TargetBay);
                        throw new StateMachineException(ErrorDescriptions.MoveExtBayNotAllowed, this.Mission.TargetBay, MessageActor.MachineManager);
                    }
                }
            }
            newStep.OnEnter(null);
        }

        private BayPosition SelectPosition(Bay currentBay)
        {
            if (this.Mission.MissionType == MissionType.OUT ||
                this.Mission.MissionType == MissionType.FullTestOUT ||
                this.Mission.MissionType == MissionType.WMS ||
                (this.Mission.MissionType == MissionType.LoadUnitOperation && this.Mission.LoadUnitSource == LoadingUnitLocation.Elevator) ||
                (this.Mission.MissionType == MissionType.FirstTest && this.Mission.LoadUnitSource == LoadingUnitLocation.Cell) ||
                (this.Mission.MissionType == MissionType.LoadUnitOperation && this.Mission.LoadUnitSource == LoadingUnitLocation.Cell) ||
                (this.Mission.MissionType == MissionType.Manual && this.Mission.LoadUnitSource == LoadingUnitLocation.Elevator) ||
                (this.Mission.MissionType == MissionType.Manual && this.Mission.LoadUnitSource == LoadingUnitLocation.Cell))
            {
                switch (this.Mission.LoadUnitDestination)
                {
                    case LoadingUnitLocation.InternalBay1Down:
                    case LoadingUnitLocation.InternalBay2Down:
                    case LoadingUnitLocation.InternalBay3Down:
                        return currentBay.Positions.SingleOrDefault(s => !s.IsUpper);

                    case LoadingUnitLocation.InternalBay1Up:
                    case LoadingUnitLocation.InternalBay2Up:
                    case LoadingUnitLocation.InternalBay3Up:
                        return currentBay.Positions.SingleOrDefault(s => s.IsUpper);

                    default:
                        return null;
                }
            }
            else
            {
                switch (this.Mission.LoadUnitSource)
                {
                    case LoadingUnitLocation.InternalBay1Down:
                    case LoadingUnitLocation.InternalBay2Down:
                    case LoadingUnitLocation.InternalBay3Down:
                        return currentBay.Positions.SingleOrDefault(s => !s.IsUpper);

                    case LoadingUnitLocation.InternalBay1Up:
                    case LoadingUnitLocation.InternalBay2Up:
                    case LoadingUnitLocation.InternalBay3Up:
                        return currentBay.Positions.SingleOrDefault(s => s.IsUpper);

                    default:
                        return null;
                }
            }
        }

        private void SetErrorMoveExtBayChain(Bay bay, BayPosition position)
        {
            this.ErrorsProvider.RecordNew(MachineErrorCode.MoveExtBayNotAllowed, bay.Number);

            //this.MachineVolatileDataProvider.Mode = MachineMode.Manual;
            this.MachineVolatileDataProvider.Mode = this.MachineVolatileDataProvider.GetMachineModeManualByBayNumber(this.Mission.TargetBay);
            this.Logger.LogInformation($"Machine status switched to {this.MachineVolatileDataProvider.Mode}");
            this.BaysDataProvider.Light(this.Mission.TargetBay, true);

            this.Mission.ErrorCode = MachineErrorCode.MoveExtBayNotAllowed;
            this.Mission.RestoreStep = this.Mission.Step;

            var newStep = new MissionMoveErrorStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        private void WakeUpMission()
        {
            var waitingMission = this.MissionsDataProvider.GetAllActiveMissions()
                .FirstOrDefault(m => m.LoadUnitSource == this.Mission.LoadUnitDestination
                    && (m.Step == MissionStep.WaitDepositCell ||
                    m.Step == MissionStep.WaitChain ||
                    m.Step == MissionStep.WaitDepositExternalBay ||
                    m.Step == MissionStep.WaitDepositInternalBay));

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
        }

        #endregion
    }
}
