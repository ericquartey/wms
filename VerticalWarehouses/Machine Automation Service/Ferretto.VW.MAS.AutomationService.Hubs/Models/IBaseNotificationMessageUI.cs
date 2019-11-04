using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.AutomationService.Hubs
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
