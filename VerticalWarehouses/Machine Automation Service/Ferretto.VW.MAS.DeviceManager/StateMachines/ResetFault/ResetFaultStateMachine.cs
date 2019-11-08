using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.ResetFault.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ResetFault.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ResetFault
{
    internal class ResetFaultStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IResetFaultMachineData machineData;

        #endregion

        #region Constructors

        public ResetFaultStateMachine(
            CommandMessage receivedMessage,
            IEnumerable<Inverter> bayInverters,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineData = new ResetFaultMachineData(
                receivedMessage.RequestingBay,
                receivedMessage.TargetBay,
                bayInverters,
                eventAggregator,
                logger,
                serviceScopeFactory);
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
            var stateData = new ResetFaultStateData(this, this.machineData);
            this.ChangeState(new ResetFaultStartState(stateData));
        }

        public override void Stop(StopRequestReason reason)
        {
            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        #endregion
    }
}
