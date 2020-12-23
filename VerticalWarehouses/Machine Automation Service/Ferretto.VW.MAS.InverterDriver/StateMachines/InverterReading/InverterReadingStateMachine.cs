using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterReading
{
    internal class InverterReadingState : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterReadingFieldMessageData inverterReadingFieldMessageData;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public InverterReadingState(
            IInverterStatusBase inverterStatus,
            IInverterReadingFieldMessageData inverterReadingFieldMessageData,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger logger)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.inverterStatus = inverterStatus;
            this.inverterReadingFieldMessageData = inverterReadingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.CurrentState = new InverterReadingStartState(this, this.inverterStatus, this.inverterReadingFieldMessageData, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion
    }
}
