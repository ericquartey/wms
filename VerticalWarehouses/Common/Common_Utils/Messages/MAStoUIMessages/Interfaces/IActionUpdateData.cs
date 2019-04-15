using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Interfaces
{
    public interface IActionUpdateData
    {
        #region Properties

        ActionStatus ActionStatus { get; set; }

        ActionType ActionType { get; set; }

        decimal? CurrentPosition { get; set; }

        NotificationType NotificationType { get; set; }

        #endregion
    }
}
