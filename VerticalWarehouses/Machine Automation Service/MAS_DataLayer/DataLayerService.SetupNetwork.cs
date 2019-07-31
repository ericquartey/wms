using System.Net;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ISetupNetworkDataLayer
    {
        #region Properties

        public IPAddress AlfaNumBay1Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.AlfaNumBay1, ConfigurationCategory.SetupNetwork);

        public IPAddress AlfaNumBay2Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.AlfaNumBay2, ConfigurationCategory.SetupNetwork);

        public IPAddress AlfaNumBay3Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.AlfaNumBay3, ConfigurationCategory.SetupNetwork);

        public IPAddress Inverter1 => this.GetIpAddressConfigurationValue((long)SetupNetwork.Inverter1, ConfigurationCategory.SetupNetwork);

        public int Inverter1Port => this.GetIntegerConfigurationValue((long)SetupNetwork.Inverter1Port, ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay1 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexBay1, ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay2 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexBay2, ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay3 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexBay3, ConfigurationCategory.SetupNetwork);

        public int InverterIndexChain => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexChain, ConfigurationCategory.SetupNetwork);

        public int InverterIndexMaster => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexMaster, ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter1 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexShutter1, ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter2 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexShutter2, ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter3 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexShutter3, ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion1 => this.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion1, ConfigurationCategory.SetupNetwork);

        public int IOExpansion1Port => this.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion1Port, ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion2 => this.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion2, ConfigurationCategory.SetupNetwork);

        public int IOExpansion2Port => this.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion2Port, ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion3 => this.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion3, ConfigurationCategory.SetupNetwork);

        public int IOExpansion3Port => this.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion3Port, ConfigurationCategory.SetupNetwork);

        public IPAddress LaserBay1Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.LaserBay1, ConfigurationCategory.SetupNetwork);

        public IPAddress LaserBay2Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.LaserBay2, ConfigurationCategory.SetupNetwork);

        public IPAddress LaserBay3Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.LaserBay3, ConfigurationCategory.SetupNetwork);

        public int MachineNumber => this.GetIntegerConfigurationValue((long)SetupNetwork.MachineNumber, ConfigurationCategory.SetupNetwork);

        public IPAddress PPC1MasterIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.PPC1MasterIPAddress, ConfigurationCategory.SetupNetwork);

        public IPAddress PPC2SlaveIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.PPC2SlaveIPAddress, ConfigurationCategory.SetupNetwork);

        public IPAddress PPC3SlaveIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.PPC3SlaveIPAddress, ConfigurationCategory.SetupNetwork);

        public IPAddress SQLServerIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.SQLServerIPAddress, ConfigurationCategory.SetupNetwork);

        public int SQLServerPort => this.GetIntegerConfigurationValue((long)SetupNetwork.SQLServerPort, ConfigurationCategory.SetupNetwork);

        public bool WMS_ON => this.GetBoolConfigurationValue((long)SetupNetwork.WMS_ON, ConfigurationCategory.SetupNetwork);

        #endregion
    }
}
