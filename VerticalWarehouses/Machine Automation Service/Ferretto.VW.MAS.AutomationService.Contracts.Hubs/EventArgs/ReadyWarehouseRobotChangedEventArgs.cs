using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class ReadyWarehouseRobotChangedEventArgs : EventArgs, IBayEventArgs
    {
        #region Constructors

        public ReadyWarehouseRobotChangedEventArgs(bool isOn, BayNumber bayNumber)
        {
            this.IsOn = isOn;
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public bool IsOn { get; }

        #endregion
    }
}
