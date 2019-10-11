using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Hubs
{
    public class NotificationEventUI<T> : PubSubEvent<NotificationMessageUI<T>>
        where T : class, IMessageData
    {
    }
}
