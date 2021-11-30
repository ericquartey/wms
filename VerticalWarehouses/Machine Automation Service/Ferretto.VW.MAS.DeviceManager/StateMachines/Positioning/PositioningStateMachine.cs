using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Positioning.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IPositioningMachineData machineData;

        #endregion

        #region Constructors

        public PositioningStateMachine(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            IPositioningMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(targetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace($"TargetPosition = {messageData.TargetPosition} - MovementType = {messageData.MovementType}");

            this.machineData = new PositioningMachineData(
                requester,
                requestingBay,
                targetBay,
                messageData,
                machineResourcesProvider,
                baysDataProvider.GetInverterIndexByMovementType(messageData, targetBay),
                eventAggregator,
                logger,
                baysDataProvider,
                serviceScopeFactory);
        }

        #endregion

        public Axis AxisMovement => this.machineData.MessageData.AxisMovement;

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status} Axis:{this.AxisMovement}");

            // We make a check about the inverter index on message and inverter index of machine data
            if (message.Source == Utils.Enumerations.FieldMessageActor.InverterDriver &&
                message.DeviceIndex != (byte)this.machineData.CurrentInverterIndex &&
                message.Type != Utils.Enumerations.FieldMessageType.MeasureProfile)
            {
                return;
            }

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status} Axis:{this.AxisMovement}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        public override void Start()
        {
            //INFO Begin check the pre conditions to start the positioning
            lock (this.CurrentState)
            {
                var stateData = new PositioningStateData(this, this.machineData);
                //INFO Check the Horizontal and Vertical conditions for Positioning
                if (this.machineData.MessageData.BypassConditions
                    || this.CheckConditions(out var errorText, out var errorCode)
                    )
                {
                    this.ChangeState(new PositioningStartState(stateData, this.Logger));
                }
                else
                {
                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                        errorsProvider.RecordNew(errorCode, this.machineData.RequestingBay);
                    }

                    var notificationMessage = new NotificationMessage(
                        this.machineData.MessageData,
                        errorText,
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.InverterException,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationStart);

                    this.PublishNotificationMessage(notificationMessage);
                    this.ChangeState(new PositioningErrorState(stateData, this.Logger));
                }
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace($"1:Stop Method: Start. Reason:{reason} Axis:{this.AxisMovement}");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        // return false in case of error
        private bool CheckConditions(out string errorText, out DataModels.MachineErrorCode errorCode)
        {
            var ok = true;
            errorText = string.Empty;
            errorCode = DataModels.MachineErrorCode.ConditionsNotMetForPositioning;
            if (this.machineData.MessageData.AxisMovement == Axis.Vertical)
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var elevatorProvider = scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
                    var verticalBounds = elevatorProvider.GetVerticalBounds();

                    if (!this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle &&
                        !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                    {
                        ok = false;
                        errorText = ErrorDescriptions.InvalidPresenceSensors;
                        errorCode = DataModels.MachineErrorCode.InvalidPresenceSensors;
                    }
                    else if (this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle && !this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    {
                        ok = false;
                        errorText = ErrorDescriptions.MissingZeroSensorWithEmptyElevator;
                        errorCode = DataModels.MachineErrorCode.MissingZeroSensorWithEmptyElevator;
                    }
                    else if (this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle &&
                            this.machineData.MachineSensorStatus.IsSensorZeroOnCradle &&
                            (this.machineData.MessageData.MovementMode == MovementMode.Position ||
                                this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasureWeight ||
                                this.machineData.MessageData.MovementMode == MovementMode.BeltBurnishing)
                        )
                    {
                        ok = false;
                        errorText = ErrorDescriptions.ZeroSensorActiveWithFullElevator;
                        errorCode = DataModels.MachineErrorCode.ZeroSensorActiveWithFullElevator;
                    }
                    else if (this.machineData.MessageData.LoadingUnitId.HasValue &&
                        !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                    {
                        ok = false;
                        errorText = ErrorDescriptions.LoadUnitPresentOnEmptyElevator;
                        errorCode = DataModels.MachineErrorCode.LoadUnitPresentOnEmptyElevator;
                    }
                    else if (this.machineData.MessageData.MovementMode == MovementMode.Position
                        && this.machineData.MachineSensorStatus.IsSensorZeroOnElevator
                        && elevatorProvider.VerticalPosition > verticalBounds.Offset * 1.1)
                    {
                        ok = false;
                        errorText = ErrorDescriptions.VerticalZeroHighError;
                        errorCode = DataModels.MachineErrorCode.VerticalZeroHighError;
                    }
                    else if (this.machineData.MessageData.MovementMode == MovementMode.Position
                        && !this.machineData.MachineSensorStatus.IsSensorZeroOnElevator
                        && elevatorProvider.VerticalPosition < verticalBounds.Offset * 0.9)
                    {
                        ok = false;
                        errorText = ErrorDescriptions.VerticalZeroLowError;
                        errorCode = DataModels.MachineErrorCode.VerticalZeroLowError;
                    }
                }
            }
            else if (this.machineData.MessageData.MovementMode == MovementMode.BayChain ||
                this.machineData.MessageData.MovementMode == MovementMode.BayTest)
            {
#if CHECK_BAY_SENSOR
                ok = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ?
                        !this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.TargetBay) :
                        !this.machineData.MachineSensorStatus.IsDrawerInBayBottom(this.machineData.TargetBay));
                if (!ok)
                {
                    errorText = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ?
                            ErrorDescriptions.TopLevelBayOccupied :
                            ErrorDescriptions.BottomLevelBayOccupied);
                    errorCode = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ?
                        DataModels.MachineErrorCode.TopLevelBayOccupied :
                        DataModels.MachineErrorCode.BottomLevelBayOccupied);
                }
                else
#endif
                {
                    ok = this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay);
                    if (!ok)
                    {
                        errorText = $"{ErrorDescriptions.SensorZeroBayNotActiveAtStart} in Bay {(int)this.machineData.TargetBay}";
                        errorCode = DataModels.MachineErrorCode.SensorZeroBayNotActiveAtStart;
                    }
                }
            }
            else if (this.machineData.MessageData.MovementMode == MovementMode.DoubleExtBayTest)
            {
                ok = this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ? this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay) : this.machineData.MachineSensorStatus.IsSensorZeroTopOnBay(this.machineData.TargetBay);
                if (!ok)
                {
                    errorText = $"{ErrorDescriptions.SensorZeroBayNotActiveAtStart} in Bay {(int)this.machineData.TargetBay}";
                    errorCode = DataModels.MachineErrorCode.SensorZeroBayNotActiveAtStart;
                }
            }
            else if (this.machineData.MessageData.MovementMode == MovementMode.BayChainFindZero)
            {
                var chainPosition = this.machineData.BaysDataProvider.GetChainPosition(this.machineData.TargetBay);
                var bay = this.machineData.BaysDataProvider.GetByNumber(this.machineData.TargetBay);
                var bayFindZeroLimit = bay.Carousel.BayFindZeroLimit;
                bayFindZeroLimit = bayFindZeroLimit == 0 ? 6 : bayFindZeroLimit;

                ok = chainPosition <= bay.Carousel.LastIdealPosition + bayFindZeroLimit && chainPosition >= bay.Carousel.LastIdealPosition - bayFindZeroLimit;
                if (!ok)
                {
                    errorText = $"{ErrorDescriptions.ConditionsNotMetForHoming} in Bay {(int)this.machineData.TargetBay}";
                    errorCode = DataModels.MachineErrorCode.ConditionsNotMetForHoming;
                }
            }
            return ok;
        }

        #endregion
    }
}
