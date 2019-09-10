using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.Utils.Messages
{
    public class CommandMessage
    {


        #region Constructors

        public CommandMessage()
        {
        }

        public CommandMessage(
            IMessageData data,
            string description,
            MessageActor destination,
            MessageActor source,
            MessageType type,
            BayNumber bayNumber,
            MessageVerbosity verbosity = MessageVerbosity.Info)
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.BayNumber = bayNumber;
            this.Verbosity = verbosity;
        }

        #endregion



        #region Properties

        public BayNumber BayNumber { get; }

        public IMessageData Data { get; }

        public string Description { get; }

        public MessageActor Destination { get; set; }

        public MessageActor Source { get; set; }

        public MessageType Type { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
