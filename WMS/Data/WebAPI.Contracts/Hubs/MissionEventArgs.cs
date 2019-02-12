using System;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public class MissionEventArgs : EventArgs
    {
        #region Constructors

        public MissionEventArgs(Mission mission)
        {
            this.Mission = mission;
        }

        #endregion

        #region Properties

        public Mission Mission { get; private set; }

        #endregion
    }
}
