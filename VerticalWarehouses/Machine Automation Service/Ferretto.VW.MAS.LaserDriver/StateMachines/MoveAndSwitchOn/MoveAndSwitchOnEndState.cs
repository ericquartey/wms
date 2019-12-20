using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.LaserDriver.StateMachines.MoveAndSwitchOn
{
    internal sealed class MoveAndSwitchOnEndState : LaserStateBase
    {
        #region Constructors

        public MoveAndSwitchOnEndState(bool success, ILaserStateMachine parentStateMachine, ILogger logger) : base(parentStateMachine, logger)
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

            var endNotification = new FieldNotificationMessage(
                null,
                "Laser Switch Off complete",
                FieldMessageActor.LaserDriver,
                FieldMessageActor.LaserDriver,
                FieldMessageType.LaserMoveAndSwitchOn,
                MessageStatus.OperationEnd,
                (byte)this.BayNumber);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
