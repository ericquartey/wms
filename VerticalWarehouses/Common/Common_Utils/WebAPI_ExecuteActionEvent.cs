using System;
using System.Collections.Generic;
using System.Text;
using Prism.Events;

namespace Ferretto.VW.Common_Utils
{
    public enum WebAPI_Action
    {
        VerticalHoming
    }

    internal class WebAPI_ExecuteActionEvent : PubSubEvent<WebAPI_Action>
    {
    }
}
