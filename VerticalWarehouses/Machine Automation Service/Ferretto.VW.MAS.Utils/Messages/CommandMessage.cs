using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages
{
    public class CommandMessage : Message
    {
        #region Constructors

        public CommandMessage()
        {
        }

        public CommandMessage(CommandMessage otherMessage)
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
            this.RequestingBay = otherMessage.RequestingBay;
            this.TargetBay = otherMessage.TargetBay;
            this.Verbosity = otherMessage.Verbosity;
        }

        public CommandMessage(
            IMessageData data,
            string description,
            MessageActor destination,
            MessageActor source,
            MessageType type,
            BayNumber requestingBay,
            BayNumber targetBay = BayNumber.None,
            MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.RequestingBay = requestingBay;
            this.TargetBay = targetBay;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public IMessageData Data { get; }

        public string Description { get; }

        public MessageActor Destination { get; set; }

        public BayNumber RequestingBay { get; }

        public MessageActor Source { get; set; }

        public BayNumber TargetBay { get; set; }

        public MessageType Type { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
