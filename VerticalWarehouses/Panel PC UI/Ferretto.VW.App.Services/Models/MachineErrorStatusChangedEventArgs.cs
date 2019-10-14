using System;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class MachineErrorStatusChangedEventArgs : System.EventArgs
    {
        #region Constructors

        public MachineErrorStatusChangedEventArgs(Error error)
        {
            this.Error = error;
        }

        #endregion

        #region Properties

        public Error Error { get; }

        #endregion
    }
}
