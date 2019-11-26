using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable LocalVariableHidesMember
namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitCloseShutterState : StateBase, IMoveLoadingUnitCloseShutterState, IProgressMessageState
    {
        #region Fields

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private IMoveLoadingUnitMessageData messageData;

        private IMoveLoadingUnitMachineData moveData;

        #endregion

        #region Constructors

        public MoveLoadingUnitCloseShutterState(
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
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData && machineData is IMoveLoadingUnitMachineData moveData)
            {
                this.messageData = messageData;
                this.moveData = moveData;
            }

            this.loadingUnitMovementProvider.CloseShutter(MessageActor.MachineManager, commandMessage.RequestingBay);
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.ShutterStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (this.messageData.EjectLoadingUnit)
                    {
                        var messageData = new MoveLoadingUnitMessageData(
                            this.messageData.MissionType,
                            this.messageData.Source,
                            this.messageData.Destination,
                            this.messageData.SourceCellId,
                            this.messageData.DestinationCellId,
                            this.messageData.LoadingUnitId,
                            this.messageData.InsertLoadingUnit,
                            this.messageData.EjectLoadingUnit,
                            this.moveData.MachineId,
                            this.messageData.CommandAction,
                            this.messageData.StopReason,
                            this.messageData.Verbosity);

                        this.Message = new NotificationMessage(
                            messageData,
                            $"Loading Unit {this.moveData.LoadingUnitId} placed on bay {this.messageData.Destination}",
                            MessageActor.AutomationService,
                            MessageActor.MachineManager,
                            MessageType.MoveLoadingUnit,
                            notification.RequestingBay,
                            notification.TargetBay,
                            MessageStatus.OperationWaitResume);

                        returnValue = this.GetState<IMoveLoadingUnitWaitEjectConfirm>();
                    }
                    else
                    {
                        returnValue = this.GetState<IMoveLoadingUnitEndState>();
                    }

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
