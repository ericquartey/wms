using System;
using Ferretto.VW.CommonUtils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public class FiniteStateMachinesEventArgs : EventArgs
    {
        #region Properties

        public Guid InstanceId { get; set; }

        public NotificationMessage NotificationMessage { get; set; }

        #endregion
    }
}
