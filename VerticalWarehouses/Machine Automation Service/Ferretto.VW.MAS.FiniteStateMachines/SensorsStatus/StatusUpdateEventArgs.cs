using System;

namespace Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus
{
    public class StatusUpdateEventArgs : EventArgs
    {


        #region Properties

        public bool NewState { get; set; }

        #endregion
    }
}
