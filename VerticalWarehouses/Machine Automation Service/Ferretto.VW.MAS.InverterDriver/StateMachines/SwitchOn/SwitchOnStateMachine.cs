using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.SwitchOn
{
    internal class SwitchOnStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly IInverterStatusBase inverterStatus;

        private bool disposed;

        #endregion

        #region Constructors

        public SwitchOnStateMachine(
            Axis axisToSwitchOn,
            IInverterStatusBase inverterStatus,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IEventAggregator eventAggregator,
            ILogger logger)
            : base(logger, eventAggregator, inverterCommandQueue)
        {
            this.inverterStatus = inverterStatus;
            this.axisToSwitchOn = axisToSwitchOn;

            this.Logger.LogDebug("1:Method Start");
        }

        #endregion

        #region Destructors

        ~SwitchOnStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new SwitchOnStartState(this, this.axisToSwitchOn, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        public override void Stop()
        {
            this.CurrentState?.Stop();
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
