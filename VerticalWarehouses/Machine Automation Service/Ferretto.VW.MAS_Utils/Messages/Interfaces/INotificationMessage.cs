using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface INotificationMessage
    {
        #region Properties

        IMessageData Data { get; }

        string Description { get; }

        MessageActor Destination { get; set; }

        ErrorLevel ErrorLevel { get; }

        MessageActor Source { get; set; }

        MessageStatus Status { get; set; }

        MessageType Type { get; }

        MessageVerbosity Verbosity { get; }

        #endregion
    }
}
