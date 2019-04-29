using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.ControlWord;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Start
{
    public class StartStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IControlWord controlWord;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public StartStateMachine(IControlWord controlWord, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.controlWord = controlWord;
            this.InverterCommandQueue = inverterCommandQueue;
            this.EventAggregator = eventAggregator;

            logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~StartStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void PublishNotificationEvent(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Type={message.Type}:Destination={message.Destination}:Status={message.Status}");

            base.PublishNotificationEvent(message);

            this.logger.LogDebug("3:Method End");
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new StartState(this, this.controlWord, this.logger);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.CurrentState.Stop();
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
