using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.Utils.Exceptions
{
    [Serializable]
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
            var notificationType = MessageType.NotSpecified;

            switch (source)
            {
                case MessageActor.MachineManager:
                    notificationType = MessageType.FsmException;
                    break;
            }

            this.NotificationMessage = new NotificationMessage(
                new FsmExceptionMessageData(this, description, 0),
                description,
                MessageActor.Any,
                source,
                notificationType,
                command.RequestingBay,
                command.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Error);
        }

        public StateMachineException(string description, BayNumber requestingBay, MessageActor source)
        {
            var notificationType = MessageType.NotSpecified;

            switch (source)
            {
                case MessageActor.MachineManager:
                    notificationType = MessageType.FsmException;
                    break;
            }

            this.NotificationMessage = new NotificationMessage(
                new FsmExceptionMessageData(this, description, 0),
                description,
                MessageActor.Any,
                source,
                notificationType,
                requestingBay,
                requestingBay,
                MessageStatus.OperationError,
                ErrorLevel.Error);
        }

        public StateMachineException() : base()
        {
        }

        public StateMachineException(string message) : base(message)
        {
        }

        public StateMachineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion

        #region Properties

        public NotificationMessage NotificationMessage { get; }

        #endregion
    }
}
