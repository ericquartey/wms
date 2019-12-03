using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ParameterHidesMember
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitStartState : StateBase, IMoveLoadingUnitStartState, IStartMessageState
    {
        #region Fields

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private bool GotoPickConfirm;

        #endregion

        #region Constructors

        public MoveLoadingUnitStartState(
                    ILoadingUnitMovementProvider loadingUnitMovementProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData
                && machineData is IMoveLoadingUnitMachineData moveData
                )
            {
                if (messageData.Source == messageData.Destination
                    && messageData.SourceCellId == null)
                {
                    this.Message = new NotificationMessage(
                        null,
                        $"Loading Unit {moveData.LoadingUnitId} is already on bay {messageData.Destination}",
                        MessageActor.MachineManager,
                        MessageActor.MachineManager,
                        MessageType.Positioning,
                        commandMessage.RequestingBay,
                        commandMessage.TargetBay,
                        MessageStatus.OperationExecuting);
                    this.GotoPickConfirm = true;
                }
                else
                {
                    var sourceHeight = this.loadingUnitMovementProvider.GetSourceHeight(messageData);

                    if (sourceHeight is null)
                    {
                        var description = $"GetSourceHeight error: position not found ({messageData.Source} {(messageData.Source == LoadingUnitLocation.Cell ? messageData.SourceCellId : messageData.LoadingUnitId)})";

                        throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                    }

                    this.loadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value, false, false, MessageActor.MachineManager, commandMessage.RequestingBay);

                    var newMessageData = new MoveLoadingUnitMessageData(
                        messageData.MissionType,
                        messageData.Source,
                        messageData.Destination,
                        messageData.SourceCellId,
                        messageData.DestinationCellId,
                        messageData.LoadingUnitId,
                        messageData.InsertLoadingUnit,
                        messageData.EjectLoadingUnit,
                        moveData.FsmId,
                        messageData.CommandAction,
                        messageData.StopReason,
                        messageData.Verbosity);

                    this.Message = new NotificationMessage(
                        newMessageData,
                        $"Loading Unit {moveData.LoadingUnitId} start movement to bay {messageData.Destination}",
                        MessageActor.AutomationService,
                        MessageActor.MachineManager,
                        MessageType.MoveLoadingUnit,
                        commandMessage.RequestingBay,
                        commandMessage.TargetBay,
                        MessageStatus.OperationStart);
                }
                moveData.FsmStateName = this.GetType().Name;
                moveData.Status = MissionStatus.Executing;
            }
            else
            {
                var description = $"Move Loading Unit Start State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            if (this.GotoPickConfirm)
            {
                return this.GetState<IMoveLoadingUnitWaitPickConfirm>();
            }
            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    returnValue = this.GetState<IMoveLoadingUnitLoadElevatorState>();
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();

                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                    ((IEndState)returnValue).ErrorMessage = notification;
                    break;
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        #endregion
    }
}
