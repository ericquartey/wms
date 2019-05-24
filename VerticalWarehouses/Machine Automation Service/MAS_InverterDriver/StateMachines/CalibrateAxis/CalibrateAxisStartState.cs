using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class CalibrateAxisStartState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public CalibrateAxisStartState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, IInverterStatusBase inverterStatus, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.inverterStatus = inverterStatus;

            
        }

        #endregion

        #region Destructors

        ~CalibrateAxisStartState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            this.inverterStatus.OperatingMode = (ushort)InverterOperationMode.Homing;

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.SetOperatingModeParam, this.inverterStatus.OperatingMode);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);

            var messageData = new CalibrateAxisFieldMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new FieldNotificationMessage(
                messageData,
                $"{this.axisToCalibrate} Homing started",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.CalibrateAxis,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"3:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.inverterStatus, this.logger));
            }

            if (message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.ParentStateMachine.ChangeState(new CalibrateAxisEnableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.inverterStatus, this.logger));
            }

            

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            

            return true;
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
