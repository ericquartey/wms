using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class BayLightChangedEventArgs : EventArgs, IBayEventArgs
    {
        #region Constructors

        public BayLightChangedEventArgs(bool isLightOn, BayNumber bayNumber)
        {
            this.IsLightOn = isLightOn;
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public bool IsLightOn { get; }

        #endregion
    }
}
