using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.EventParameters;
using Prism.Events;

namespace Ferretto.VW.Common_Utils.Events
{
    public class WebAPI_CommandEvent : PubSubEvent<Command_EventParameter>
    {
    }
}
