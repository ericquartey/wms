using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionStartState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public MissionStartState(IStateMachine parentMachine, ILogger logger)
        {
            logger.LogDebug( "1:Method Start" );

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
        }

        #endregion

        #region Properties

        public override string Type => "MissionStartState";

        #endregion

        /// <inheritdoc/>

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action
                    this.ProcessStopAction( message );
                    break;

                    //case MessageType.EndAction:
                    //    //TODO add state business logic to end current action
                    //    this.ProcessEndAction(message);
                    //    break;

                    //case MessageType.ErrorAction:
                    //    this.ProcessErrorAction(message);
                    //    break;
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            //if (message.Type == MessageType.EndAction)
            //{
            //    switch (message.Status)
            //    {
            //        case MessageStatus.OperationEnd:
            //            //TEMP Change to mission end state after the Mission is done successfully
            //            this.ParentStateMachine.ChangeState(new MissionEndState(this.ParentStateMachine, this.logger), null);
            //            break;

            //        case MessageStatus.OperationError:
            //            //TEMP Change to error state when an error has occurred
            //            this.ParentStateMachine.ChangeState(new MissionErrorState(this.ParentStateMachine, this.logger), null);
            //            break;

            //        default:
            //            break;
            //    }
            //}

            //if (message.Type == MessageType.ErrorAction)
            //{
            //    switch (message.Status)
            //    {
            //        case MessageStatus.OperationError:
            //            {
            //                //TEMP Change to error state when an error has occurred
            //                this.ParentStateMachine.ChangeState(new MissionErrorState(this.ParentStateMachine, this.logger), null);
            //                break;
            //            }

            //        default:
            //            {
            //                break;
            //            }
            //    }
            //}
        }

        public override void Start()
        {
            this.logger.LogDebug( "1:Method Start" );

            //var newMessage = new CommandMessage(null,
            //    "Mission State Started",
            //    MessageActor.Any,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.StartAction,
            //    MessageVerbosity.Info);
            //this.ParentStateMachine.PublishCommandMessage(newMessage);

            //var inverterMessage = new CommandMessage(null,
            //    "Mission State Started",
            //    MessageActor.InverterDriver,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.StartAction,
            //    MessageVerbosity.Info);
            //this.ParentStateMachine.PublishCommandMessage(newMessage);
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        private void ProcessEndAction(CommandMessage message)
        {
            //var newMessage = new CommandMessage(null,
            //    "End Mission",
            //    MessageActor.Any,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.EndAction,
            //    MessageVerbosity.Info);
            //this.ParentStateMachine.ChangeState(new MissionEndState(this.ParentStateMachine, this.logger), newMessage);
        }

        private void ProcessErrorAction(CommandMessage message)
        {
            var newMessage = new CommandMessage( null,
                "Stop Requested",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info );
            this.ParentStateMachine.ChangeState( new MissionErrorState( this.ParentStateMachine, this.logger ), newMessage );
        }

        private void ProcessStopAction(CommandMessage message)
        {
            var newMessage = new CommandMessage( null,
                "Stop Requested",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                MessageVerbosity.Info );
            this.ParentStateMachine.ChangeState( new MissionEndState( this.ParentStateMachine, this.logger ), newMessage );
        }

        #endregion
    }
}
