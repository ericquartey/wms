using System;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class EndMissionRobotChangedEventArgs : EventArgs, IBayEventArgs
    {
        #region Constructors

        public EndMissionRobotChangedEventArgs(bool isOn, BayNumber bayNumber)
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
