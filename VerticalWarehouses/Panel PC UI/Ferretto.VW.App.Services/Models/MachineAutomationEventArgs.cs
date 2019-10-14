using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;

namespace Ferretto.VW.App.Services.Models
{
    public class MachineAutomationEventArgs
    {
        #region Constructors

        public MachineAutomationEventArgs(
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
