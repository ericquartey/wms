using Ferretto.VW.Common_Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.Common_Utils.Events
{
    public class CommandEvent : PubSubEvent<CommandMessage>
    {
    }
}
