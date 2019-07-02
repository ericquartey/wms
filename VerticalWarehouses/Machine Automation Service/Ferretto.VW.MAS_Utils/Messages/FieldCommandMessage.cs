using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_Utils.Messages
{
    public class FieldCommandMessage
    {
        #region Constructors

        public FieldCommandMessage()
        {
            this.Type = FieldMessageType.NoType;
        }

        public FieldCommandMessage(
            IFieldMessageData data,
            string description,
            FieldMessageActor destination,
            FieldMessageActor source,
            FieldMessageType type,
            byte deviceIndex = 0x00)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.DeviceIndex = deviceIndex;
        }

        #endregion

        #region Properties

        public IFieldMessageData Data { get; }

        public string Description { get; }

        public FieldMessageActor Destination { get; set; }

        public byte DeviceIndex { get; set; }

        public FieldMessageActor Source { get; set; }

        public FieldMessageType Type { get; }

        #endregion
    }
}
