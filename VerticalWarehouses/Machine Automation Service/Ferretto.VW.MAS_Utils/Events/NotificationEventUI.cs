using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.Utils.Events
{
    public class NotificationEventUI<T> : PubSubEvent<NotificationMessageUI<T>>
        where T : class, IMessageData
    {
    }
}
