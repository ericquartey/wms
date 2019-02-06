using Prism.Events;

namespace Ferretto.Common.Common_Utils
{
    public enum WebAPI_Action
    {
        VerticalHoming
    }

    public class WebAPI_ExecuteActionEvent : PubSubEvent<WebAPI_Action>
    {
    }
}
