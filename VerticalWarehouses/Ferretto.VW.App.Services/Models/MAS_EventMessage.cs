using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;

namespace Ferretto.VW.App.Services
{
    public class MAS_EventMessage
    {
        #region Constructors

        public MAS_EventMessage(
            NotificationType notificationType,
            ActionType actionType,
            ActionStatus actionStatus,
            INotificationMessageData data = null)
        {
            this.ActionType = actionType;
            this.ActionStatus = actionStatus;
            this.NotificationType = notificationType;
            this.Data = data;
        }

        #endregion

        #region Properties

        public ActionStatus ActionStatus { get; set; }

        public ActionType ActionType { get; set; }

        public INotificationMessageData Data { get; set; }

        public NotificationType NotificationType { get; set; }

        #endregion
    }
}
