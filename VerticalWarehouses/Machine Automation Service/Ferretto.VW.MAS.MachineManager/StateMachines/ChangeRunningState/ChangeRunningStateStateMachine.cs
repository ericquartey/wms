using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState
{
    internal class ChangeRunningStateStateMachine : FiniteStateMachine<IChangeRunningStateStartState, IChangeRunningStateStartState>, IChangeRunningStateStateMachine
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public ChangeRunningStateStateMachine(
            IMachineControlProvider machineControlProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger)
            : base(eventAggregator, logger)
        {
            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));

            this.MachineData = new ChangeRunningStateMachineData(this.InstanceId);
        }

        #endregion

        #region Methods

        public override bool AllowMultipleInstances(CommandMessage command)
        {
            return false;
        }

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Type == MessageType.ChangeRunningState;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return this.machineControlProvider.FilterNotifications(notification, MessageActor.MachineManager);
        }

        protected override IState OnCommandReceived(CommandMessage commandMessage)
        {
            var newState = base.OnCommandReceived(commandMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            newState = this.ActiveState.CommandReceived(commandMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        protected override IState OnNotificationReceived(NotificationMessage notificationMessage)
        {
            var newState = base.OnNotificationReceived(notificationMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            newState = this.ActiveState.NotificationReceived(notificationMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        protected override bool OnStart(CommandMessage commandMessage, CancellationToken cancellationToken)
        {
            var returnValue = false;

            if (this.CheckStartConditions(commandMessage))
            {
                returnValue = true;
            }
            else
            {
                this.errorsProvider.RecordNew(MachineErrorCode.ConditionsNotMetForRunning, commandMessage.RequestingBay);
            }

            return returnValue;
        }

        private bool CheckStartConditions(CommandMessage commandMessage)
        {
            var returnValue = false;
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                if (messageData.Enable)
                {
                    returnValue = this.sensorsProvider.IsMachineSecureForRun();
                }
                else
                {
                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
