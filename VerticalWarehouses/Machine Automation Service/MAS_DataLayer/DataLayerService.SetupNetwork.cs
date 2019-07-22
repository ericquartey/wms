using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService : ISetupNetwork
    {
        #region Properties

        public Task<IPAddress> AlfaNumBay1Net => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.AlfaNumBay1, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> AlfaNumBay2Net => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.AlfaNumBay2, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> AlfaNumBay3Net => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.AlfaNumBay3, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> Inverter1 => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.Inverter1, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> Inverter1Port => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.Inverter1Port, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexBay1 => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexBay1, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexBay2 => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexBay2, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexBay3 => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexBay3, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexChain => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexChain, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexMaster => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexMaster, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexShutter1 => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexShutter1, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexShutter2 => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexShutter2, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> InverterIndexShutter3 => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.InverterIndexShutter3, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> IOExpansion1 => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion1, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> IOExpansion1Port => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion1Port, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> IOExpansion2 => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion2, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> IOExpansion2Port => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion2Port, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> IOExpansion3 => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.IOExpansion3, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> IOExpansion3Port => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.IOExpansion3Port, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> LaserBay1Net => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.LaserBay1, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> LaserBay2Net => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.LaserBay2, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> LaserBay3Net => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.LaserBay3, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> MachineNumber => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.MachineNumber, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> PPC1MasterIPAddress => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.PPC1MasterIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> PPC2SlaveIPAddress => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.PPC2SlaveIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> PPC3SlaveIPAddress => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.PPC3SlaveIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public Task<IPAddress> SQLServerIPAddress => this.GetIPAddressConfigurationValueAsync((long)SetupNetwork.SQLServerIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public Task<int> SQLServerPort => this.GetIntegerConfigurationValueAsync((long)SetupNetwork.SQLServerPort, (long)ConfigurationCategory.SetupNetwork);

        public Task<bool> WMS_ON => this.GetBoolConfigurationValueAsync((long)SetupNetwork.WMS_ON, (long)ConfigurationCategory.SetupNetwork);

        #endregion
    }
}
