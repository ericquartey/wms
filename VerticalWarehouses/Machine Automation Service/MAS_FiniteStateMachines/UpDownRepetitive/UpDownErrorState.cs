using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownErrorState : StateBase
    {
        #region Fields

        private readonly IUpDownRepetitiveMessageData upDownMessageData;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public UpDownErrorState(IStateMachine parentMachine, IUpDownRepetitiveMessageData upDownMessageData, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentMachine;
            this.upDownMessageData = upDownMessageData;
            this.logger = logger;

            

            
        }

        #endregion

        #region Properties

        public override string Type => "UpDownErrorState";

        #endregion

        #region Methods

        /// <inheritdoc/>

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            //TEMP Notify the error condition
            //var newMessage = new NotificationMessage(null,
            //    "Up&Down Error State",
            //    MessageActor.Any,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.Positioning,
            //    MessageStatus.OperationError,
            //    ErrorLevel.Error,
            //    MessageVerbosity.Info);
            //this.ParentStateMachine.PublishNotificationMessage(newMessage);

            
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            //TEMP Add your implementation code here
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            //TEMP Add your implementation code here
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
