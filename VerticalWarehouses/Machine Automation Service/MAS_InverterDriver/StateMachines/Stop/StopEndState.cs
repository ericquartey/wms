using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Stop
{
    public class StopEndState : InverterStateBase
    {
        #region Constructors

        public StopEndState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
        }

        #endregion

        #region Destructors

        ~StopEndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        public override void Start()
        {
            //TODO: Check this snippet code about the setting of control word value: there is a mismatch about the actually controlWord set to the Inverter
            if (this.InverterStatus is IAngInverterStatus angInverter)
            {
                var horizontalAxis = (this.InverterStatus.CommonControlWord.Value & 0x8000) > 0;
                if (horizontalAxis)
                {
                    this.InverterStatus.CommonControlWord.Value = 0x8000;
                }
                else
                {
                    this.InverterStatus.CommonControlWord.Value = 0x0000;
                }
            }

            if (this.InverterStatus is IAglInverterStatus aglInverter)
            {
                // TODO
            }

            if (this.InverterStatus is IAcuInverterStatus acuInverter)
            {
                // TODO
            }

            //this.InverterStatus.CommonControlWord.Value = 0x0000;

            Enum.TryParse(this.InverterStatus.SystemIndex.ToString(), out InverterIndex inverterIndex);

            var notificationMessageData = new InverterStopFieldMessageData(inverterIndex);
            var notificationMessage = new FieldNotificationMessage(
                notificationMessageData,
                "Inverter Stop End",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterStop,
                MessageStatus.OperationEnd);

            this.Logger.LogTrace($"1:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
