using System;
using System.Collections.Generic;
using System.Text;
using Prism.Events;

namespace Ferretto.VW.Common_Utils.Events
{
    public class InverterDriver_NotificationEvent : PubSubEvent<InverterDriver_Notification>
    {
    }
    public enum InverterDriver_Notification
    {
        Error,
        End

    }
}
