using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.Common_Utils.Messages
{
    public class NotificationMessage
    {
        #region Constructors

        public NotificationMessage()
        {
        }

        public NotificationMessage(IMessageData data,
            String description,
            MessageActor destination,
            MessageActor source,
            MessageType type,
            MessageStatus status,
            MessageVerbosity verbosity,
            ErrorLevel level = ErrorLevel.NoError)
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

        public String Description { get; }

        public ErrorLevel ErrorLevel { get; }

        public MessageType Type { get; }

        public MessageVerbosity Verbosity { get; }

        public MessageActor Destination { get; set; }

        public MessageActor Source { get; set; }

        public MessageStatus Status { get; private set; }

        #endregion
    }
}
