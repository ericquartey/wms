using System;

namespace Ferretto.VW.Simulator.Services
{
    public class HorizontalMovementEventArgs : EventArgs
    {
        #region Properties

        public bool IsLoading { get; set; }

        public bool IsLoadingExternal { get; set; }

        #endregion
    }
}
