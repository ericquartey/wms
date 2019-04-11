using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS_Utils.Events
{
    public class NotificationEventUI<U> : PubSubEvent<NotificationMessageUI<U>>
        where U : class, IMessageData
    {
    }
}
