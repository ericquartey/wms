using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Interfaces
{
    public interface IActionUpdateData
    {
        #region Properties

        ActionStatus ActionStatus { get; set; }

        ActionType ActionType { get; set; }

        double? CurrentPosition { get; set; }

        NotificationType NotificationType { get; set; }

        #endregion
    }
}
