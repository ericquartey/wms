using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public PositioningTableStateMachine(
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            if (data is null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }

            if (inverterStatus is null)
            {
                throw new System.ArgumentNullException(nameof(inverterStatus));
            }

            this.data = data;
            this.inverterStatus = inverterStatus;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new PositioningTableStartState(this, this.data, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.CurrentState?.Stop();
        }

        #endregion
    }
}
