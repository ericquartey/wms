using System.Net;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ISetupNetworkDataLayer
    {
        #region Properties

        IPAddress AlfaNumBay1Net { get; }

        IPAddress AlfaNumBay2Net { get; }

        IPAddress AlfaNumBay3Net { get; }

        IPAddress Inverter1 { get; }

        int Inverter1Port { get; }

        int InverterIndexBay1 { get; }

        int InverterIndexBay2 { get; }

        int InverterIndexBay3 { get; }

        int InverterIndexChain { get; }

        int InverterIndexMaster { get; }

        int InverterIndexShutter1 { get; }

        int InverterIndexShutter2 { get; }

        int InverterIndexShutter3 { get; }

        IPAddress IOExpansion1IPAddress { get; }

        int IOExpansion1Port { get; }

        IPAddress IOExpansion2IPAddress { get; }

        int IOExpansion2Port { get; }

        IPAddress IOExpansion3IPAddress { get; }

        int IOExpansion3Port { get; }

        int MachineNumber { get; }

        IPAddress PPC1MasterIPAddress { get; }

        IPAddress PPC2SlaveIPAddress { get; }

        IPAddress PPC3SlaveIPAddress { get; }

        IPAddress SQLServerIPAddress { get; }

        int SQLServerPort { get; }

        #endregion
    }
}
