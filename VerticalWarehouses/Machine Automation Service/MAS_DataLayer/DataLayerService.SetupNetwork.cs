using System.Net;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ISetupNetwork
    {
        #region Properties

        public IPAddress AlfaNumBay1Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.AlfaNumBay1, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress AlfaNumBay2Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.AlfaNumBay2, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress AlfaNumBay3Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.AlfaNumBay3, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress Inverter1 => this.GetIpAddressConfigurationValue((long)SetupNetwork.Inverter1, (long)ConfigurationCategory.SetupNetwork);

        public int Inverter1Port => this.GetIntegerConfigurationValue((long)SetupNetwork.Inverter1Port, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay1 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexBay1, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay2 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexBay2, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexBay3 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexBay3, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexChain => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexChain, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexMaster => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexMaster, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter1 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexShutter1, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter2 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexShutter2, (long)ConfigurationCategory.SetupNetwork);

        public int InverterIndexShutter3 => this.GetIntegerConfigurationValue((long)SetupNetwork.InverterIndexShutter3, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion1 => this.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion1, (long)ConfigurationCategory.SetupNetwork);

        public int IOExpansion1Port => this.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion1Port, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion2 => this.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion2, (long)ConfigurationCategory.SetupNetwork);

        public int IOExpansion2Port => this.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion2Port, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress IOExpansion3 => this.GetIpAddressConfigurationValue((long)SetupNetwork.IOExpansion3, (long)ConfigurationCategory.SetupNetwork);

        public int IOExpansion3Port => this.GetIntegerConfigurationValue((long)SetupNetwork.IOExpansion3Port, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress LaserBay1Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.LaserBay1, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress LaserBay2Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.LaserBay2, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress LaserBay3Net => this.GetIpAddressConfigurationValue((long)SetupNetwork.LaserBay3, (long)ConfigurationCategory.SetupNetwork);

        public int MachineNumber => this.GetIntegerConfigurationValue((long)SetupNetwork.MachineNumber, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress PPC1MasterIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.PPC1MasterIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress PPC2SlaveIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.PPC2SlaveIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress PPC3SlaveIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.PPC3SlaveIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public IPAddress SQLServerIPAddress => this.GetIpAddressConfigurationValue((long)SetupNetwork.SQLServerIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public bool WMS_ON => this.GetBoolConfigurationValue((long)SetupNetwork.WMS_ON, (long)ConfigurationCategory.SetupNetwork);
        public int SQLServerPort => this.GetIntegerConfigurationValue((long)SetupNetwork.SQLServerPort, (long)ConfigurationCategory.SetupNetwork);

       
        #endregion
    }
}
