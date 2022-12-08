using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Statistics.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Statistics.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Statistics
{
    internal class StatisticsStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IStatisticsMachineData machineData;

        #endregion

        #region Constructors

        public StatisticsStateMachine(
            CommandMessage receivedMessage,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.machineData = new StatisticsMachineData(
                receivedMessage.RequestingBay,
                receivedMessage.TargetBay,
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
            var stateData = new StatisticsStateData(this, this.machineData);
            this.ChangeState(new StatisticsStartState(stateData, this.Logger));
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
