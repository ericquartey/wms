using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages
{
    public class NotificationMessageUI<TData> : IBaseNotificationMessageUI
            where TData : class, IMessageData
    {
        #region Properties

        public TData Data { get; set; }

        public string Description { get; set; }

        public MessageActor Destination { get; set; }

        public ErrorLevel ErrorLevel { get; set; } = ErrorLevel.NoError;

        public MessageActor Source { get; set; }

        public MessageStatus Status { get; set; }

        public MessageType Type { get; set; }

        public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Info;

        #endregion
    }
}
