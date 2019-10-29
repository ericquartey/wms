using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.Utils.Exceptions
{
    public class StateMachineException : Exception
    {
        #region Constructors

        public StateMachineException(string description, NotificationMessage notificationMessage)
            : base(description)
        {
            this.NotificationMessage = notificationMessage;
        }

        public StateMachineException(string description, CommandMessage command, MessageActor source)
        {
            MessageType notificationType = MessageType.NotSpecified;

            switch (source)
            {
                case MessageActor.MachineManager:
                    notificationType = MessageType.MachineManagerException;
                    break;
            }

            this.NotificationMessage = new NotificationMessage(
                command.Data,
                description,
                MessageActor.Any,
                source,
                notificationType,
                command.RequestingBay,
                command.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Critical);
        }

        #endregion

        #region Properties

        public NotificationMessage NotificationMessage { get; }

        #endregion
    }
}
