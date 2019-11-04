using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Positioning.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
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
            ILogger<DeviceManager> logger,
            IBaysProvider baysProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace($"TargetPosition = {messageData.TargetPosition} - CurrentPosition = {messageData.CurrentPosition} - MovementType = {messageData.MovementType}");

            this.machineData = new PositioningMachineData(
                requester,
                requestingBay,
                targetBay,
                messageData,
                machineResourcesProvider,
                baysProvider.GetInverterIndexByMovementType(messageData, targetBay),
                eventAggregator,
                logger,
                baysProvider,
                serviceScopeFactory);
        }

        #endregion

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
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

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
                if (this.CheckConditions(out var errorText, out var errorCode))
                {
                    if (this.machineData.MessageData.MovementMode == MovementMode.FindZero
                        &&
                        this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                    {
                        this.ChangeState(new PositioningEndState(stateData));
                    }
                    else
                    {
                        this.ChangeState(new PositioningStartState(stateData));
                    }
                }
                else
                {
                    var notificationMessage = new NotificationMessage(
                        this.machineData.MessageData,
                        errorText,
                        MessageActor.Any,
                        MessageActor.DeviceManager,
                        MessageType.InverterException,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationStart);

                    using (var scope = this.ServiceScopeFactory.CreateScope())
                    {
                        var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                        errorsProvider.RecordNew(errorCode, this.machineData.RequestingBay);
                    }

                    this.Logger.LogError(errorText);

                    this.PublishNotificationMessage(notificationMessage);
                    this.ChangeState(new PositioningErrorState(stateData));
                }
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

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
                if (!this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle &&
                   !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                {
                    ok = false;
                    errorText = "Invalid presence sensors";
                    errorCode = DataModels.MachineErrorCode.InvalidPresenceSensors;
                }
                else if (this.machineData.MachineSensorStatus.IsDrawerCompletelyOffCradle && !this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                {
                    ok = false;
                    errorText = "Missing Zero sensor with empty elevator";
                    errorCode = DataModels.MachineErrorCode.MissingZeroSensorWithEmptyElevator;
                }
                else if (this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle &&
                        this.machineData.MachineSensorStatus.IsSensorZeroOnCradle &&
                        (this.machineData.MessageData.MovementMode == MovementMode.Position ||
                            this.machineData.MessageData.MovementMode == MovementMode.PositionAndMeasure ||
                            this.machineData.MessageData.MovementMode == MovementMode.BeltBurnishing)
                    )
                {
                    ok = false;
                    errorText = "Zero sensor active with full elevator";
                    errorCode = DataModels.MachineErrorCode.ZeroSensorActiveWithFullElevator;
                }
                else if (this.machineData.MessageData.LoadingUnitId.HasValue &&
                    !this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle)
                {
                    ok = false;
                    errorText = "Load Unit present on empty Elevator";
                    errorCode = DataModels.MachineErrorCode.LoadUnitPresentOnEmptyElevator;
                }
            }
            else if (this.machineData.MessageData.MovementMode == MovementMode.BayChain)
            {
                ok = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ?
                        !this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.TargetBay) :
                        !this.machineData.MachineSensorStatus.IsDrawerInBayBottom(this.machineData.TargetBay));
                if (!ok)
                {
                    errorText = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ?
                            $"Top level Bay {(int)this.machineData.TargetBay} Occupied" :
                            $"Bottom level Bay {(int)this.machineData.TargetBay} Occupied");
                    errorCode = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ?
                        DataModels.MachineErrorCode.TopLevelBayOccupied :
                        DataModels.MachineErrorCode.BottomLevelBayOccupied);
                }
                else
                {
                    ok = this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay);
                    if (!ok)
                    {
                        errorText = $"Sensor zero Bay {(int)this.machineData.TargetBay} not active at start";
                        errorCode = DataModels.MachineErrorCode.SensoZeroBayNotActiveAtStart;
                    }
                }
            }
            return ok;
        }

        #endregion
    }
}
