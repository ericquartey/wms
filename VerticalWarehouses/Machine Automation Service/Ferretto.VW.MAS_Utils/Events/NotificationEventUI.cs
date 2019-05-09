using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS_Utils.Events
{
    public class NotificationEventUI<T> : PubSubEvent<NotificationMessageUI<T>>
        where T : class, IMessageData
    {
    }
}
