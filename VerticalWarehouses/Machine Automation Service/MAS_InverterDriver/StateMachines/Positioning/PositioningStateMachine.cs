using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterStatusBase inverterStatus;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningStateMachine(
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IEventAggregator eventAggregator,
            ILogger logger)
            : base(logger, eventAggregator, inverterCommandQueue)
        {
            this.data = data;
            this.inverterStatus = inverterStatus;

            this.Logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Destructors

        ~PositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            // Start test code
            if (this.inverterStatus is AngInverterStatus currentStatus)
            {
                this.Logger.LogTrace($"1:CurrentPositionAxisVertical = {currentStatus.CurrentPositionAxisVertical}");
                this.Logger.LogTrace($"2:data.TargetPosition = {this.data.TargetPosition}");

                if (currentStatus.CurrentPositionAxisVertical != this.data.TargetPosition)
                {
                    this.CurrentState = new PositioningStartState(this, this.data, this.inverterStatus, this.Logger);
                }
                else
                {
                    this.CurrentState = new PositioningEndState(this, this.inverterStatus, this.Logger);
                }

                this.CurrentState?.Start();
            }
            // End test code

            //this.CurrentState = new PositioningStartState(this, this.data, this.inverterStatus, this.Logger);
            //this.CurrentState?.Start();
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
