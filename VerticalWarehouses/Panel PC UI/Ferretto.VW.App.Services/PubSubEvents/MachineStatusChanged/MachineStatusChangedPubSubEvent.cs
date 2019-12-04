using Prism.Events;

namespace Ferretto.VW.App.Services
{
    public class MachineStatusChangedPubSubEvent : PubSubEvent<MachineStatusChangedMessage>
    {
    }
}
