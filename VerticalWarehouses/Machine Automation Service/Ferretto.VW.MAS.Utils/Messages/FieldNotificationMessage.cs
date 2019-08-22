using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.Utils.Messages
{
    public class FieldNotificationMessage
    {


        #region Constructors

        public FieldNotificationMessage()
        {
        }

        public FieldNotificationMessage(FieldNotificationMessage otherMessage)
        {
            if (otherMessage == null)
            {
                throw new System.ArgumentNullException(nameof(otherMessage));
            }

            this.Data = otherMessage.Data;
            this.Description = otherMessage.Description;
            this.Destination = otherMessage.Destination;
            this.Source = otherMessage.Source;
            this.Type = otherMessage.Type;
            this.Status = otherMessage.Status;
            this.ErrorLevel = otherMessage.ErrorLevel;
        }

        public FieldNotificationMessage(
            IFieldMessageData data,
            string description,
            FieldMessageActor destination,
            FieldMessageActor source,
            FieldMessageType type,
            MessageStatus status,
            byte deviceIndex,
            ErrorLevel level = ErrorLevel.NoError)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.DeviceIndex = deviceIndex;
            this.Status = status;
            this.ErrorLevel = level;
        }

        #endregion



        #region Properties

        public IFieldMessageData Data { get; }

        public string Description { get; }

        public FieldMessageActor Destination { get; set; }

        public byte DeviceIndex { get; set; }

        public ErrorLevel ErrorLevel { get; }

        public FieldMessageActor Source { get; set; }

        public MessageStatus Status { get; set; }

        public FieldMessageType Type { get; }

        #endregion
    }
}
