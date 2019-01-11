using System;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class MissionEventArgs : EventArgs
    {
        #region Constructors

        public MissionEventArgs(Mission mission)
        {
            this.Mission = mission;
        }

        #endregion Constructors

        #region Properties

        public Mission Mission { get; private set; }

        #endregion Properties
    }
}
