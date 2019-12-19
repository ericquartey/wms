using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.LaserDriver.StateMachines.MoveAndSwitchOn
{
    internal sealed class MoveAndSwitchOnStartState : LaserStateBase
    {
        #region Constructors

        public MoveAndSwitchOnStartState(ILaserStateMachine parentStateMachine, ILogger logger) : base(parentStateMachine, logger)
        {
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(string message)
        {
            this.Logger.LogTrace("1:Method Start");

            if (message == "OK")
            {
                this.ParentStateMachine.ChangeState(new MoveAndSwitchOnMovingState(this.ParentStateMachine, this.Logger));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new MoveAndSwitchOnEndState(false, this.ParentStateMachine, this.Logger));
            }
        }

        public override void Start()
        {
            this.ParentStateMachine.EnqueueMessage(new FieldCommandMessage(null, string.Empty, FieldMessageActor.LaserDriver, FieldMessageActor.LaserDriver, FieldMessageType.LaserOff, (byte)this.BayNumber));
        }

        #endregion
    }
}
