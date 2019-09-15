using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.ResetFault
{
    internal class ResetFaultStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly InverterIndex inverterIndex;

        private readonly IInverterStatusBase inverterStatus;

        #endregion

        #region Constructors

        public ResetFaultStateMachine(
            IInverterStatusBase inverterStatus,
            InverterIndex inverterIndex,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IEventAggregator eventAggregator,
            ILogger logger)
            : base(logger, eventAggregator, inverterCommandQueue)
        {
            this.inverterStatus = inverterStatus;
            this.inverterIndex = inverterIndex;
            this.Logger.LogDebug("1:Method Start");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new ResetFaultStartState(this, this.inverterStatus, this.inverterIndex, this.Logger);
            this.CurrentState?.Start();
        }

        public override void Stop()
        {
            this.CurrentState?.Stop();
        }

        #endregion
    }
}
