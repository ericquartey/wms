using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable
{
    internal class PowerEnableStateMachine : StateMachineBase
    {

        #region Fields

        private readonly IPowerEnableMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStateMachine(
                    CommandMessage receivedMessage,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory
            )
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            if(receivedMessage.Data is IPowerEnableMessageData data)
            {
                this.machineData = new PowerEnableMachineData(data.Enable, receivedMessage.RequestingBay, receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory);
            }
        }

        #endregion



        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock(this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            lock(this.CurrentState)
            {
                var stateData = new PowerEnableStateData(this, this.machineData);
                this.CurrentState = new PowerEnableStartState(stateData);
                this.CurrentState?.Start();
            }

            this.Logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock(this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(this.disposed)
            {
                return;
            }

            if(disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
