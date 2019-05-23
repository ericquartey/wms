using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningEnableOperationState : InverterStateBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private readonly ILogger logger;

        private readonly IShutterPositioningFieldMessageData shutterPositionData;

        protected BlockingConcurrentQueue<InverterMessage> InverterCommandQueue;

        private bool targetReachedReset;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningEnableOperationState(IInverterStateMachine parentStateMachine, IInverterStatusBase inverterStatus, IShutterPositioningFieldMessageData shutterPositionData, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.inverterStatus = inverterStatus;
            this.shutterPositionData = shutterPositionData;

            
        }

        #endregion

        #region Destructors

        ~ShutterPositioningEnableOperationState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");
            if (this.inverterStatus is AglInverterStatus currentStatus)
            {
                currentStatus.CommonControlWord.EnableOperation = true;
            }

            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, this.shutterPositionData.ShutterPosition));

            var inverterMessage = new InverterMessage(this.inverterStatus.SystemIndex, (short)InverterParameterId.ControlWordParam, ((AglInverterStatus)this.inverterStatus).ProfileVelocityControlWord.Value);

            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);


        }

        /// <inheritdoc/>
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");
            
             return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            var returnValue = false;

            if (message.IsError)
            {
                this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.inverterStatus, this.shutterPositionData, this.logger));
            }

            this.inverterStatus.CommonStatusWord.Value = message.UShortPayload;

            if (this.inverterStatus.CommonStatusWord.IsOperationEnabled)
            {               
                if (this.inverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    this.ParentStateMachine.ChangeState(new ShutterPositioningDisableOperationState(this.ParentStateMachine, this.inverterStatus, this.shutterPositionData, this.logger));
                    returnValue = true;
                }                 
            }

            

            return returnValue;
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
