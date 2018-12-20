using System;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class MissionEventArgs : EventArgs
    {
        #region Constructors

        public MissionEventArgs(Core.Mission mission)
        {
            this.Mission = mission;
        }

        #endregion Constructors

        #region Properties

        public Core.Mission Mission { get; private set; }

        #endregion Properties
    }
}
