using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages
{
    public class FieldNotificationMessage
    {
        #region Constructors

        public FieldNotificationMessage()
        {
        }

        public FieldNotificationMessage(FieldNotificationMessage other)
        {
            this.Data = other.Data;
            this.Description = other.Description;
            this.Destination = other.Destination;
            this.Source = other.Source;
            this.Type = other.Type;
            this.Status = other.Status;
            this.ErrorLevel = other.ErrorLevel;
        }

        public FieldNotificationMessage(IFieldMessageData data,
            string description,
            FieldMessageActor destination,
            FieldMessageActor source,
            FieldMessageType type,
            MessageStatus status,
            ErrorLevel level = ErrorLevel.NoError)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.Status = status;
            this.ErrorLevel = level;
        }

        #endregion

        #region Properties

        public IFieldMessageData Data { get; }

        public string Description { get; }

        public FieldMessageActor Destination { get; set; }

        public ErrorLevel ErrorLevel { get; }

        public FieldMessageActor Source { get; set; }

        public MessageStatus Status { get; set; }

        public FieldMessageType Type { get; }

        #endregion
    }
}
