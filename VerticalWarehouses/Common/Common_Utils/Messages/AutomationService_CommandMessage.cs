using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages
{
    public class AutomationService_CommandMessage
    {
        public AutomationCommandType AutomationCommand { get; set; }
    }

    public enum AutomationCommandType
    {
        HorizontalHoming
    }
}
