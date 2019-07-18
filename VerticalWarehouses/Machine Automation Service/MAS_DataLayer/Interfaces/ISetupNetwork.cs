using System.Net;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface ISetupNetwork
    {
        #region Properties

        Task<IPAddress> AlfaNumBay1Net { get; }

        Task<IPAddress> AlfaNumBay2Net { get; }

        Task<IPAddress> AlfaNumBay3Net { get; }

        Task<IPAddress> Inverter1 { get; }

        Task<int> Inverter1Port { get; }

        Task<int> InverterIndexBay1 { get; }

        Task<int> InverterIndexBay2 { get; }

        Task<int> InverterIndexBay3 { get; }

        Task<int> InverterIndexChain { get; }

        Task<int> InverterIndexMaster { get; }

        Task<int> InverterIndexShutter1 { get; }

        Task<int> InverterIndexShutter2 { get; }

        Task<int> InverterIndexShutter3 { get; }

        Task<IPAddress> IOExpansion1 { get; }

        Task<int> IOExpansion1Port { get; }

        Task<IPAddress> IOExpansion2 { get; }

        Task<int> IOExpansion2Port { get; }

        Task<IPAddress> IOExpansion3 { get; }

        Task<int> IOExpansion3Port { get; }

        Task<IPAddress> LaserBay1Net { get; }

        Task<IPAddress> LaserBay2Net { get; }

        Task<IPAddress> LaserBay3Net { get; }

        Task<int> MachineNumber { get; }

        Task<IPAddress> PPC1MasterIPAddress { get; }

        Task<IPAddress> PPC2SlaveIPAddress { get; }

        Task<IPAddress> PPC3SlaveIPAddress { get; }

        Task<IPAddress> SQLServerIPAddress { get; }

        Task<int> SQLServerPort { get; }

        Task<bool> WMS_ON { get; }

        #endregion
    }
}
