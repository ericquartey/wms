using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.MAStoUIMessages
{
    public class ActionUpdateData : IActionUpdateData
    {
        #region Constructors

        public ActionUpdateData(NotificationType notificationType, ActionType actionType, ActionStatus actionStatus, decimal? currentPosition = null)
        {
            this.NotificationType = notificationType;
            this.ActionType = actionType;
            this.ActionStatus = actionStatus;
            this.CurrentPosition = currentPosition;
        }

        #endregion

        #region Properties

        public ActionStatus ActionStatus { get; set; }

        public ActionType ActionType { get; set; }

        public decimal? CurrentPosition { get; set; }

        public NotificationType NotificationType { get; set; }

        #endregion
    }
}
