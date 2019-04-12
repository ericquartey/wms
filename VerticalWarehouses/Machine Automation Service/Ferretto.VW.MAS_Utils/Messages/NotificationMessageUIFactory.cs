using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages
{
    public static class NotificationMessageUIFactory
    {
        #region Methods

        /// <summary>
        /// Convert the input NotificationMessage into a NotificationMessageUI object via reflection.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     Message data type is null: it cannot be instantiated.
        /// </exception>
        public static IBaseNotificationMessageUI FromNotificationMessage(NotificationMessage input)
        {
            var space = typeof(NotificationMessageUIFactory).Namespace;
            var messageType = $"{space}.Data.{input.Type}MessageData";

            var messageDataType = typeof(NotificationMessageUIFactory).Assembly.GetType(messageType);

            if (messageDataType == null)
            {
                throw new System.ArgumentNullException(nameof(messageDataType), "Message data type for UI cannot be created.");
            }

            var genericMessageUIType = typeof(NotificationMessageUI<>).MakeGenericType(messageDataType);

            var notificationMessage = System.Activator.CreateInstance(genericMessageUIType) as IBaseNotificationMessageUI;

            notificationMessage.Description = input.Description;
            notificationMessage.Destination = input.Destination;
            notificationMessage.Source = input.Source;
            notificationMessage.Type = input.Type;
            notificationMessage.Status = input.Status;
            notificationMessage.ErrorLevel = input.ErrorLevel;
            notificationMessage.Verbosity = input.Verbosity;

            genericMessageUIType.GetProperty(nameof(NotificationMessage.Data)).SetValue(notificationMessage, input.Data);

            return notificationMessage;
        }

        #endregion
    }
}
