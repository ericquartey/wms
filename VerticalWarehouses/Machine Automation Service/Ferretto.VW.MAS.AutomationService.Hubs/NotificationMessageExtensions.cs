using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public static class NotificationMessageExtensions
    {
        #region Methods

        public static bool IsErrored(this IBaseNotificationMessageUI message)
        {
            if (message is null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            return
                message.Status == MessageStatus.OperationError ||
                message.Status == MessageStatus.OperationFaultStop;
        }

        public static bool IsNotRunning(this IBaseNotificationMessageUI message)
        {
            if (message is null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            return
                message.Status == MessageStatus.OperationEnd ||
                message.Status == MessageStatus.OperationError ||
                message.Status == MessageStatus.OperationFaultStop ||
                message.Status == MessageStatus.OperationRunningStop ||
                message.Status == MessageStatus.OperationStop;
        }

        #endregion
    }
}
