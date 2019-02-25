using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages
{

    public class CommandMessage
    {
        #region Constructors

        public CommandMessage()
        {
        }

        public CommandMessage( IMessageData data,
            string description,
            MessageActor destination,
            MessageActor source,
            MessageVerbosity verbosity,
            MessageType type )
        {
            this.Data = data;
            this.Description = description;
            this.Destination = destination;
            this.Source = source;
            this.Type = type;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public IMessageData Data { get; private set; }

        public string Description { get; private set; }

        public MessageActor Destination { get; set; }

        public MessageActor Source { get; set; }

        public MessageType Type { get; private set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
