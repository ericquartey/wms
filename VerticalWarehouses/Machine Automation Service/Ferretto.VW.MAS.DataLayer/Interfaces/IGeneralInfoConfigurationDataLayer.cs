using System;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IGeneralInfoConfigurationDataLayer
    {
        #region Properties

        bool AlfaNumBay1 { get; }

        bool AlfaNumBay2 { get; }

        bool AlfaNumBay3 { get; }

        int Barrier1Height { get; }

        int Barrier2Height { get; }

        int Barrier3Height { get; }

        bool LaserBay1 { get; }

        bool LaserBay2 { get; }

        bool LaserBay3 { get; }

        int Shutter1Type { get; }

        int Shutter2Type { get; }

        int Shutter3Type { get; }

        #endregion
    }
}
