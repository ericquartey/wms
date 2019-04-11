using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages
{
    //public interface IBaseNotificationMessage
    //{
    //    #region Properties

    //    string Description { get; }

    //    MessageActor Destination { get; set; }

    //    ErrorLevel ErrorLevel { get; }

    //    MessageActor Source { get; set; }

    //    MessageStatus Status { get; set; }

    //    MessageType Type { get; }

    //    MessageVerbosity Verbosity { get; }

    //    #endregion
    //}

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

        public NotificationMessage(FieldNotificationMessage fieldNotificationMessage)
        {
            this.Data = this.GetAutomationData(fieldNotificationMessage.Data);
            this.Description = fieldNotificationMessage.Description;
            this.Type = this.GetAutomationType(fieldNotificationMessage.Type);
            this.Status = fieldNotificationMessage.Status;
            this.ErrorLevel = fieldNotificationMessage.ErrorLevel;
        }

        #endregion

        #region Properties

        public IMessageData Data { get; }

        public string Description { get; }

        public MessageActor Destination { get; set; }

        public ErrorLevel ErrorLevel { get; }

        public MessageActor Source { get; set; }

        public MessageStatus Status { get; set; }

        public MessageType Type { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        private IMessageData GetAutomationData(IFieldMessageData data)
        {
            IMessageData returnValue = null;
            if (data is ISwitchAxisFieldMessageData messageData)
            {
                returnValue = new HomingMessageData(messageData);
            }

            return returnValue;
        }

        private MessageType GetAutomationType(FieldMessageType type)
        {
            MessageType returnValue;

            switch (type)
            {
                case FieldMessageType.CalibrateAxis:
                    returnValue = MessageType.Homing;
                    break;

                default:
                    returnValue = MessageType.NoType;
                    break;
            }

            return returnValue;
        }

        #endregion
    }

    //public class NotificationMessage2<TData> : IBaseNotificationMessage
    //    where TData : class, IMessageData
    //{
    //    #region Properties

    //    public TData Data { get; set; }

    //    public string Description { get; set; }

    //    public MessageActor Destination { get; set; }

    //    public ErrorLevel ErrorLevel { get; set; } = ErrorLevel.NoError;

    //    public MessageActor Source { get; set; }

    //    public MessageStatus Status { get; set; }

    //    public MessageType Type { get; set; }

    //    public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Info;

    //    #endregion
    //}
}
