using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ISetupNetwork
    {
        #region Properties

        public string AlfaNumBay1Net => this.GetStringConfigurationValue((long)SetupNetwork.AlfaNumBay1Net, (long)ConfigurationCategory.SetupNetwork);

        public string AlfaNumBay2Net => this.GetStringConfigurationValue((long)SetupNetwork.AlfaNumBay2Net, (long)ConfigurationCategory.SetupNetwork);

        public string AlfaNumBay3Net => this.GetStringConfigurationValue((long)SetupNetwork.AlfaNumBay3Net, (long)ConfigurationCategory.SetupNetwork);

        public string Inverter1 => this.GetStringConfigurationValue((long)SetupNetwork.Inverter1, (long)ConfigurationCategory.SetupNetwork);

        public string Inverter1Port => this.GetStringConfigurationValue((long)SetupNetwork.Inverter1Port, (long)ConfigurationCategory.SetupNetwork);

        public string Inverter2 => this.GetStringConfigurationValue((long)SetupNetwork.Inverter2, (long)ConfigurationCategory.SetupNetwork);

        public string Inverter2Port => this.GetStringConfigurationValue((long)SetupNetwork.Inverter2Port, (long)ConfigurationCategory.SetupNetwork);

        public string IOExpansion1 => this.GetStringConfigurationValue((long)SetupNetwork.IOExpansion1, (long)ConfigurationCategory.SetupNetwork);

        public string IOExpansion1Port => this.GetStringConfigurationValue((long)SetupNetwork.IOExpansion1Port, (long)ConfigurationCategory.SetupNetwork);

        public string IOExpansion2 => this.GetStringConfigurationValue((long)SetupNetwork.IOExpansion2, (long)ConfigurationCategory.SetupNetwork);

        public string IOExpansion2Port => this.GetStringConfigurationValue((long)SetupNetwork.IOExpansion2Port, (long)ConfigurationCategory.SetupNetwork);

        public string IOExpansion3 => this.GetStringConfigurationValue((long)SetupNetwork.IOExpansion3, (long)ConfigurationCategory.SetupNetwork);

        public string IOExpansion3Port => this.GetStringConfigurationValue((long)SetupNetwork.IOExpansion3Port, (long)ConfigurationCategory.SetupNetwork);

        public string LaserBay1Net => this.GetStringConfigurationValue((long)SetupNetwork.LaserBay1Net, (long)ConfigurationCategory.SetupNetwork);

        public string LaserBay2Net => this.GetStringConfigurationValue((long)SetupNetwork.LaserBay2Net, (long)ConfigurationCategory.SetupNetwork);

        public string LaserBay3Net => this.GetStringConfigurationValue((long)SetupNetwork.LaserBay3Net, (long)ConfigurationCategory.SetupNetwork);

        public int MachineNumber => this.GetIntegerConfigurationValue((long)SetupNetwork.MachineNumber, (long)ConfigurationCategory.SetupNetwork);

        public string PPC1MasterIPAddress => this.GetStringConfigurationValue((long)SetupNetwork.PPC1MasterIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public string PPC2SlaveIPAddress => this.GetStringConfigurationValue((long)SetupNetwork.PPC2SlaveIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public string PPC3SlaveIPAddress => this.GetStringConfigurationValue((long)SetupNetwork.PPC3SlaveIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public string SQLServerIPAddress => this.GetStringConfigurationValue((long)SetupNetwork.SQLServerIPAddress, (long)ConfigurationCategory.SetupNetwork);

        public bool WMS_ON => this.GetBoolConfigurationValue((long)SetupNetwork.WMS_ON, (long)ConfigurationCategory.SetupNetwork);

        #endregion
    }
}
