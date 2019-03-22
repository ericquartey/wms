using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages
{
    public class NotificationMessage
    {
        #region Constructors

        public NotificationMessage()
        {
        }

        public NotificationMessage(IMessageData data,
            string description,
            MessageActor destination,
            MessageActor source,
            MessageType type,
            MessageStatus status,
            ErrorLevel level = ErrorLevel.NoError,
            MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.Status = status;
            this.Verbosity = verbosity;
            this.ErrorLevel = level;
        }

        #endregion

        #region Properties

        public IMessageData Data { get; }

        public string Description { get; }

        public MessageActor Destination { get; set; }

        public ErrorLevel ErrorLevel { get; }

        public MessageActor Source { get; set; }

        public MessageStatus Status { get; }

        public MessageType Type { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
