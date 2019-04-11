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
        public static IBaseNotificationMessageUI FromNotificationMessage(NotificationMessage input)
        {
            var space = typeof(NotificationMessageUIFactory).Namespace;
            var messageType = $"{space}.Data.{input.Type}MessageData";

            var messageDataType = typeof(NotificationMessageUIFactory).Assembly.GetType(messageType);

            if (messageDataType == null)
            {
                return null;
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
