using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.EventParameters
{
    public class Notification_EventParameter
    {
        public OperationStatus OperationStatus { get; set; }
        public OperationType OperationType { get; set; }
    }

    public enum OperationStatus
    {
        Error,
        End
    }

    public enum OperationType
    {
        Homing
    }
}
