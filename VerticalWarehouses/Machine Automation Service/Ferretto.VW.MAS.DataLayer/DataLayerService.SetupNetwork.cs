using System.Net;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ISetupNetworkDataLayer
    {
        #region Properties

        public IPAddress AlfaNumBay1Net => this.GetIpAddressConfigurationValue(SetupNetwork.AlfaNumBay1, ConfigurationCategory.SetupNetwork);

        public IPAddress AlfaNumBay2Net => this.GetIpAddressConfigurationValue(SetupNetwork.AlfaNumBay2, ConfigurationCategory.SetupNetwork);

        public IPAddress AlfaNumBay3Net => this.GetIpAddressConfigurationValue(SetupNetwork.AlfaNumBay3, ConfigurationCategory.SetupNetwork);

        public IPAddress Inverter1 => this.GetIpAddressConfigurationValue(SetupNetwork.Inverter1, ConfigurationCategory.SetupNetwork);

        public int Inverter1Port => this.GetIntegerConfigurationValue(SetupNetwork.Inverter1Port, ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay1 => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexBay1, ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay2 => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexBay2, ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay3 => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexBay3, ConfigurationCategory.SetupNetwork);

        public int InverterIndexChain => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexChain, ConfigurationCategory.SetupNetwork);

        public int InverterIndexMaster => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexMaster, ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter1 => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexShutter1, ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter2 => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexShutter2, ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter3 => this.GetIntegerConfigurationValue(SetupNetwork.InverterIndexShutter3, ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion1IPAddress => this.GetIpAddressConfigurationValue(SetupNetwork.IOExpansion1IPAddress, ConfigurationCategory.SetupNetwork);

        public int IOExpansion1Port => this.GetIntegerConfigurationValue(SetupNetwork.IOExpansion1Port, ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion2IPAddress => this.GetIpAddressConfigurationValue(SetupNetwork.IOExpansion2IPAddress, ConfigurationCategory.SetupNetwork);

        public int IOExpansion2Port => this.GetIntegerConfigurationValue(SetupNetwork.IOExpansion2Port, ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion3IPAddress => this.GetIpAddressConfigurationValue(SetupNetwork.IOExpansion3IPAddress, ConfigurationCategory.SetupNetwork);

        public int IOExpansion3Port => this.GetIntegerConfigurationValue(SetupNetwork.IOExpansion3Port, ConfigurationCategory.SetupNetwork);

        public int MachineNumber => this.GetIntegerConfigurationValue(SetupNetwork.MachineNumber, ConfigurationCategory.SetupNetwork);

        public IPAddress PPC1MasterIPAddress => this.GetIpAddressConfigurationValue(SetupNetwork.PPC1MasterIPAddress, ConfigurationCategory.SetupNetwork);

        public IPAddress PPC2SlaveIPAddress => this.GetIpAddressConfigurationValue(SetupNetwork.PPC2SlaveIPAddress, ConfigurationCategory.SetupNetwork);

        public IPAddress PPC3SlaveIPAddress => this.GetIpAddressConfigurationValue(SetupNetwork.PPC3SlaveIPAddress, ConfigurationCategory.SetupNetwork);

        public IPAddress SQLServerIPAddress => this.GetIpAddressConfigurationValue(SetupNetwork.SQLServerIPAddress, ConfigurationCategory.SetupNetwork);

        public int SQLServerPort => this.GetIntegerConfigurationValue(SetupNetwork.SQLServerPort, ConfigurationCategory.SetupNetwork);

        public bool WMS_ON => this.GetBoolConfigurationValue(SetupNetwork.WMS_ON, ConfigurationCategory.SetupNetwork);

        #endregion
    }
}
