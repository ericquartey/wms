using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.LaserDriver.StateMachines
{
    internal abstract class LaserStateBase : ILaserState
    {
        #region Constructors

        public LaserStateBase(ILaserStateMachine parentStateMachine, ILogger logger)
        {
            this.ParentStateMachine = parentStateMachine;
            this.Logger = logger;
        }

        #endregion

        #region Properties

        protected BayNumber BayNumber => this.ParentStateMachine?.BayNumber ?? BayNumber.None;

        public virtual string Type => this.GetType().ToString();

        protected ILogger Logger { get; }

        protected ILaserStateMachine ParentStateMachine { get; }

        #endregion

        #region Methods

        public abstract void ProcessResponseMessage(string message);

        public abstract void Start();

        #endregion
    }
}
