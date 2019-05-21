using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionErrorState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public MissionErrorState(IStateMachine parentMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;

            this.logger.LogDebug("4:Method End");
        }

        #endregion

        #region Properties

        public override string Type => "MissionErrorState";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Start()
        {
            logger.LogDebug("1:Method Start");

            //var newMessage = new CommandMessage(null,
            //    "Mission State Ending",
            //    MessageActor.Any,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.EndAction,
            //    MessageVerbosity.Info);
            //this.ParentStateMachine.PublishCommandMessage(newMessage);

            this.logger.LogDebug("4:Method End");
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO add state business logic to stop current action

                    var newMessage = new CommandMessage(null,
                        "Mission Error",
                        MessageActor.Any,
                        MessageActor.FiniteStateMachines,
                        MessageType.Stop,
                        MessageVerbosity.Info);
                    this.ParentStateMachine.PublishCommandMessage(newMessage);
                    break;
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
