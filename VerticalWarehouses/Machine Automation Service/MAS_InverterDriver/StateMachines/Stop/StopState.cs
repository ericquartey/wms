using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.Stop
{
    public class StopState : InverterStateBase
    {
        #region Fields

        private const ushort StatusWordValue = 0x0050;

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public StopState(IInverterStateMachine parentStateMachine, Axis axisToStop, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;
            this.axisToStop = axisToStop;

            switch (this.axisToStop)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x8000;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x0000;
                    break;
            }
            var stopMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            parentStateMachine.EnqueueMessage(stopMessage);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            var returnValue = false;

            //TEMP this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - VoltageDisabledState:ProcessMessage");
            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToStop, this.logger));
            }
            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                if ((message.UShortPayload & StatusWordValue) == StatusWordValue)
                {
                    var messageData = new StopAxisMessageData(this.axisToStop);
                    var endNotification = new NotificationMessage(messageData, "Axis calibration complete", MessageActor.Any,
                        MessageActor.InverterDriver, MessageType.Stop, MessageStatus.OperationEnd);
                    this.parentStateMachine.PublishNotificationEvent(endNotification);
                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
