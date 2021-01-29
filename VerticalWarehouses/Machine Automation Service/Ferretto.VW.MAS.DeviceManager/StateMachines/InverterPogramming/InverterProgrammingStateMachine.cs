using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Interfaces;
using Ferretto.VW.MAS.DeviceManager.InverterProgramming.Models;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.InverterPogramming
{
    internal class InverterProgrammingStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IInverterProgrammingMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public InverterProgrammingStateMachine(
            CommandMessage receivedMessage,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory)
        {
            if (receivedMessage.Data is IInverterProgrammingMessageData data)
            {
                this.machineData = new InverterProgrammingMachineData(data.InverterParametersData,
                    receivedMessage.RequestingBay,
                    receivedMessage.TargetBay,
                    eventAggregator,
                    logger,
                    serviceScopeFactory);
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.CurrentState.ProcessCommandMessage(message);
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            lock (this.CurrentState)
            {
                var stateData = new InverterProgrammingStateData(this, this.machineData);
                this.ChangeState(new InverterProgrammingStartState(stateData, this.Logger));
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"Stop with reason: {reason}");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
