using Prism.Events;

namespace Ferretto.Common.Common_Utils
{
    public class TestHomingEvent : PubSubEvent
    {
    }

    public class WebAPI_ExecuteActionEvent : PubSubEvent<string>
    {
    }
}
