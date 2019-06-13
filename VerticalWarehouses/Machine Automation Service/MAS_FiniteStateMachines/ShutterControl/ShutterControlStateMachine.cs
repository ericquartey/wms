using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterControl
{
    public class ShutterControlStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IShutterControlMessageData shutterControlMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterControlStateMachine(IEventAggregator eventAggregator, IShutterControlMessageData shutterControlMessageData, ILogger logger)
            : base(eventAggregator, logger)
        {
            this.logger = logger;

            this.logger.LogTrace("1:Method Start");

            this.CurrentState = new EmptyState(logger);

            this.shutterControlMessageData = shutterControlMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterControlStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
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
