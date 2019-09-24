using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IGeneralInfoConfigurationDataLayer
    {
        #region Properties

        public bool AlfaNumBay1 => this.GetBoolConfigurationValue(GeneralInfo.AlfaNumBay1, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay2 => this.GetBoolConfigurationValue(GeneralInfo.AlfaNumBay2, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay3 => this.GetBoolConfigurationValue(GeneralInfo.AlfaNumBay3, ConfigurationCategory.GeneralInfo);

        public int Barrier1Height => this.GetIntegerConfigurationValue(GeneralInfo.Barrier1Height, ConfigurationCategory.GeneralInfo);

        public int Barrier2Height => this.GetIntegerConfigurationValue(GeneralInfo.Barrier2Height, ConfigurationCategory.GeneralInfo);

        public int Barrier3Height => this.GetIntegerConfigurationValue(GeneralInfo.Barrier3Height, ConfigurationCategory.GeneralInfo);

        public bool LaserBay1 => this.GetBoolConfigurationValue(GeneralInfo.LaserBay1, ConfigurationCategory.GeneralInfo);

        public bool LaserBay2 => this.GetBoolConfigurationValue(GeneralInfo.LaserBay2, ConfigurationCategory.GeneralInfo);

        public bool LaserBay3 => this.GetBoolConfigurationValue(GeneralInfo.LaserBay3, ConfigurationCategory.GeneralInfo);

        public int Shutter1Type => this.GetIntegerConfigurationValue(GeneralInfo.Shutter1Type, ConfigurationCategory.GeneralInfo);

        public int Shutter2Type => this.GetIntegerConfigurationValue(GeneralInfo.Shutter2Type, ConfigurationCategory.GeneralInfo);

        public int Shutter3Type => this.GetIntegerConfigurationValue(GeneralInfo.Shutter3Type, ConfigurationCategory.GeneralInfo);

        #endregion
    }
}
