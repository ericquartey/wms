using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.LaserDriver.StateMachines.SwitchOff
{
    internal sealed class SwitchOffEndState : LaserStateBase
    {
        #region Constructors

        public SwitchOffEndState(bool success, ILaserStateMachine parentStateMachine, ILogger logger) : base(parentStateMachine, logger)
        {
            this.Success = success;
        }

        #endregion

        #region Properties

        private bool Success { get; set; }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(string message)
        {
            this.Logger.LogTrace($"1: Received Message = {message}");
        }

        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        #endregion
    }
}
