using System.Diagnostics;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using NLog;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages
{
    public class FieldCommandMessage
    {
        #region Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public FieldCommandMessage()
        {
            this.Type = FieldMessageType.NoType;
        }

        public FieldCommandMessage(FieldCommandMessage otherMessage)
        {
            if (otherMessage is null)
            {
                throw new System.ArgumentNullException(nameof(otherMessage));
            }

            this.Data = otherMessage.Data;
            this.Description = otherMessage.Description;
            this.Destination = otherMessage.Destination;
            this.Source = otherMessage.Source;
            this.Type = otherMessage.Type;
            this.DeviceIndex = otherMessage.DeviceIndex;
        }

        public FieldCommandMessage(
            IFieldMessageData data,
            string description,
            FieldMessageActor destination,
            FieldMessageActor source,
            FieldMessageType type,
            byte deviceIndex)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.DeviceIndex = deviceIndex;

            if (Logger?.IsTraceEnabled ?? false)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                var trace = $"{sf.GetMethod().ReflectedType?.Name}.{sf.GetMethod().Name}()";

                Logger.Trace($"{source} -> {destination} - type:{type} description:\"{description}\" [{data}][{trace}]");
            }
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
