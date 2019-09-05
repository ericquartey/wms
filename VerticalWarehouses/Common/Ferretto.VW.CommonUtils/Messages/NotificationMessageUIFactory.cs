using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages
{
    public static class NotificationMessageUiFactory
    {
        #region Methods

        /// <summary>
        /// Converts the input NotificationMessage into a NotificationMessageUI object via reflection.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>A <c>IBaseNotificationMessageUI</c> instance reflecting the specified message.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     Message data type is null: it cannot be instantiated.
        /// </exception>
        public static IBaseNotificationMessageUI FromNotificationMessage(NotificationMessage message)
        {
            if (message == null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            var space = typeof(NotificationMessageUiFactory).Namespace;
            var messageType = $"{space}.Data.{message.Type}MessageData";

            var messageDataType = typeof(NotificationMessageUiFactory).Assembly.GetType(messageType);
            if (messageDataType == null)
            {
                throw new System.InvalidOperationException(
                    $"Message data type '{messageType}' cannot be found.");
            }

            var genericMessageUiType = typeof(NotificationMessageUI<>).MakeGenericType(messageDataType);

            var notificationMessage = System.Activator.CreateInstance(genericMessageUiType) as IBaseNotificationMessageUI;

            notificationMessage.Description = message.Description;
            notificationMessage.Destination = message.Destination;
            notificationMessage.Source = message.Source;
            notificationMessage.Type = message.Type;
            notificationMessage.Status = message.Status;
            notificationMessage.ErrorLevel = message.ErrorLevel;
            notificationMessage.Verbosity = message.Verbosity;

            genericMessageUiType.GetProperty(nameof(NotificationMessage.Data)).SetValue(notificationMessage, message.Data);

            return notificationMessage;
        }

        #endregion
    }
}
