using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_IODriver.Interface;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SetConfiguration
{
    public class SetConfigurationErrorState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public SetConfigurationErrorState(IIoStateMachine parentStateMachine, IoSHDStatus status, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;
        }

        #endregion

        #region Destructors

        ~SetConfigurationErrorState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogTrace("1:Method Start");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug($"1: Received Message = {message.ToString()}");
        }

        public override void Start()
        {
            this.logger.LogTrace("1:Method Start");
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
