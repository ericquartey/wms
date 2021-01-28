using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterProgramming
{
    internal class InverterProgrammigState : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public InverterProgrammigState(
            IInverterStatusBase inverterStatus,
            IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger logger)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.inverterStatus = inverterStatus;
            this.inverterProgrammingFieldMessageData = inverterProgrammingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.CurrentState = new InverterProgrammingStartState(this, this.inverterStatus, this.EventAggregator, this.inverterProgrammingFieldMessageData, this.Logger);
            this.CurrentState?.Start();
        }

        #endregion
    }
}
