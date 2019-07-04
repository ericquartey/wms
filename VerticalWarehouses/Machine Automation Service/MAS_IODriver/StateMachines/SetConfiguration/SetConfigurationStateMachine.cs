using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationStateMachine : IoStateMachineBase
    {
        #region Fields

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationStateMachine(BlockingConcurrentQueue<IoSHDWriteMessage> ioCommandQueue, IoSHDStatus status, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.IoCommandQueue = ioCommandQueue;
            this.status = status;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Destructors

        ~SetConfigurationStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            base.ProcessMessage(message);
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogTrace("1:Method Start");

            base.ProcessResponseMessage(message);
        }

        public override void Start()
        {
            this.CurrentState = new SetConfigurationStartState(this, this.status, this.logger);
            this.CurrentState?.Start();
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
