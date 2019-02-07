using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.EventParameters
{
    public enum OperationStatus
    {
        Error,

        End
    }

    public enum OperationType
    {
        Homing
    }

    public class Notification_EventParameter
    {
        #region Properties

        public string Description { get; set; }

        public OperationStatus OperationStatus { get; set; }

        public OperationType OperationType { get; set; }

        #endregion
    }
}
