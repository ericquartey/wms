using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.LaserDriver.StateMachines
{
    internal abstract class LaserStateBase : ILaserState
    {
        #region Properties

        public virtual string Type => this.GetType().ToString();

        protected ILogger Logger { get; }

        protected ILaserStateMachine ParentStateMachine { get; }

        #endregion

        #region Methods

        public void ProcessResponseMessage(string message)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
