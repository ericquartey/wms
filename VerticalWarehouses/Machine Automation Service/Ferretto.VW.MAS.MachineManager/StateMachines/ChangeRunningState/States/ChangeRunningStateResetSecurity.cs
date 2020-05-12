using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateResetSecurity : StateBase, IChangeRunningStateResetSecurity
    {
        #region Fields

        private readonly IMachineControlProvider machineControlProvider;

        #endregion

        #region Constructors

        public ChangeRunningStateResetSecurity(
            IMachineControlProvider machineControlProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.machineControlProvider = machineControlProvider ??
                throw new ArgumentNullException(nameof(machineControlProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"ChangeRunningStateResetSecurity: received command {commandMessage.Type}, {commandMessage.Description}");
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                if (messageData.Enable)
                {
                    this.machineControlProvider.ResetSecurity(MessageActor.MachineManager, commandMessage.RequestingBay);
                }
                else
                {
                    var description = "Power Enable Reset Fault State started during machine power down";

                    throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                }
            }
            else
            {
                var description = $"Power Enable Reset Fault State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.ResetSecurityStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        returnValue = this.GetState<IChangeRunningStateInverterPowerSwitch>();
                        break;

                    case MessageStatus.OperationError:
                        returnValue = this.GetState<IChangeRunningStateEndState>();

                        ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                        ((IEndState)returnValue).ErrorMessage = notification;
                        break;
                }
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"ChangeRunningStateStartState: OnStop.");
            var returnValue = this.GetState<IChangeRunningStateEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        #endregion
    }
}
