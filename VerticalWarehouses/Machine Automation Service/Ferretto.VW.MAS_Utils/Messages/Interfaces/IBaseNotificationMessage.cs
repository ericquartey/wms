using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IBaseNotificationMessageUI
    {
        #region Properties

        string Description { get; set; }

        MessageActor Destination { get; set; }

        ErrorLevel ErrorLevel { get; set; }

        MessageActor Source { get; set; }

        MessageStatus Status { get; set; }

        MessageType Type { get; set; }

        MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
