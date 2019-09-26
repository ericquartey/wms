using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IGeneralInfoConfigurationDataLayer
    {


        #region Properties

        public string Address => this.GetStringConfigurationValue(GeneralInfo.Address, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay1 => this.GetBoolConfigurationValue(GeneralInfo.AlfaNumBay1, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay2 => this.GetBoolConfigurationValue(GeneralInfo.AlfaNumBay2, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay3 => this.GetBoolConfigurationValue(GeneralInfo.AlfaNumBay3, ConfigurationCategory.GeneralInfo);

        public int Barrier1Height => this.GetIntegerConfigurationValue(GeneralInfo.Barrier1Height, ConfigurationCategory.GeneralInfo);

        public int Barrier2Height => this.GetIntegerConfigurationValue(GeneralInfo.Barrier2Height, ConfigurationCategory.GeneralInfo);

        public int Barrier3Height => this.GetIntegerConfigurationValue(GeneralInfo.Barrier3Height, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Height1 => this.GetDecimalConfigurationValue(GeneralInfo.Bay1Height1, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Height2 => this.GetDecimalConfigurationValue(GeneralInfo.Bay1Height2, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Position1 => this.GetDecimalConfigurationValue(GeneralInfo.Bay1Position1, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Position2 => this.GetDecimalConfigurationValue(GeneralInfo.Bay1Position2, ConfigurationCategory.GeneralInfo);

        public int Bay1Type => this.GetIntegerConfigurationValue(GeneralInfo.Bay1Type, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Height1 => this.GetDecimalConfigurationValue(GeneralInfo.Bay2Height1, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Height2 => this.GetDecimalConfigurationValue(GeneralInfo.Bay2Height2, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Position1 => this.GetDecimalConfigurationValue(GeneralInfo.Bay2Position1, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Position2 => this.GetDecimalConfigurationValue(GeneralInfo.Bay2Position2, ConfigurationCategory.GeneralInfo);

        public int Bay2Type => this.GetIntegerConfigurationValue(GeneralInfo.Bay2Type, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Height1 => this.GetDecimalConfigurationValue(GeneralInfo.Bay3Height1, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Height2 => this.GetDecimalConfigurationValue(GeneralInfo.Bay3Height2, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Position1 => this.GetDecimalConfigurationValue(GeneralInfo.Bay3Position1, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Position2 => this.GetDecimalConfigurationValue(GeneralInfo.Bay3Position2, ConfigurationCategory.GeneralInfo);

        public int Bay3Type => this.GetIntegerConfigurationValue(GeneralInfo.Bay3Type, ConfigurationCategory.GeneralInfo);

        public int BaysQuantity => this.GetIntegerConfigurationValue(GeneralInfo.BaysQuantity, ConfigurationCategory.GeneralInfo);

        public string City => this.GetStringConfigurationValue(GeneralInfo.City, ConfigurationCategory.GeneralInfo);

        public string ClientCode => this.GetStringConfigurationValue(GeneralInfo.ClientCode, ConfigurationCategory.GeneralInfo);

        public string ClientName => this.GetStringConfigurationValue(GeneralInfo.ClientName, ConfigurationCategory.GeneralInfo);

        public string Country => this.GetStringConfigurationValue(GeneralInfo.Country, ConfigurationCategory.GeneralInfo);

        public decimal Height => this.GetDecimalConfigurationValue(GeneralInfo.Height, ConfigurationCategory.GeneralInfo);

        public DateTime InstallationDate => this.GetDateTimeConfigurationValue(GeneralInfo.InstallationDate, ConfigurationCategory.GeneralInfo);

        public bool LaserBay1 => this.GetBoolConfigurationValue(GeneralInfo.LaserBay1, ConfigurationCategory.GeneralInfo);

        public bool LaserBay2 => this.GetBoolConfigurationValue(GeneralInfo.LaserBay2, ConfigurationCategory.GeneralInfo);

        public bool LaserBay3 => this.GetBoolConfigurationValue(GeneralInfo.LaserBay3, ConfigurationCategory.GeneralInfo);

        public string Latitude => this.GetStringConfigurationValue(GeneralInfo.Latitude, ConfigurationCategory.GeneralInfo);

        public string Longitude => this.GetStringConfigurationValue(GeneralInfo.Longitude, ConfigurationCategory.GeneralInfo);

        public int MaxAcceptedBai1Height => this.GetIntegerConfigurationValue(GeneralInfo.MaxAcceptedBai1Height, ConfigurationCategory.GeneralInfo);

        public int MaxAcceptedBai2Height => this.GetIntegerConfigurationValue(GeneralInfo.MaxAcceptedBai2Height, ConfigurationCategory.GeneralInfo);

        public int MaxAcceptedBai3Height => this.GetIntegerConfigurationValue(GeneralInfo.MaxAcceptedBai3Height, ConfigurationCategory.GeneralInfo);

        public int MaxDrawerGrossWeight => this.GetIntegerConfigurationValue(GeneralInfo.MaxDrawerGrossWeight, ConfigurationCategory.GeneralInfo);

        public double MaxGrossWeight => this.GetIntegerConfigurationValue(GeneralInfo.MaxGrossWeight, ConfigurationCategory.GeneralInfo);

        public string Model => this.GetStringConfigurationValue(GeneralInfo.Model, ConfigurationCategory.GeneralInfo);

        public string Order => this.GetStringConfigurationValue(GeneralInfo.Order, ConfigurationCategory.GeneralInfo);

        public DateTime ProductionDate => this.GetDateTimeConfigurationValue(GeneralInfo.ProductionDate, ConfigurationCategory.GeneralInfo);

        public string Province => this.GetStringConfigurationValue(GeneralInfo.Province, ConfigurationCategory.GeneralInfo);

        public string Serial => this.GetStringConfigurationValue(GeneralInfo.Serial, ConfigurationCategory.GeneralInfo);

        public int Shutter1Type => this.GetIntegerConfigurationValue(GeneralInfo.Shutter1Type, ConfigurationCategory.GeneralInfo);

        public int Shutter2Type => this.GetIntegerConfigurationValue(GeneralInfo.Shutter2Type, ConfigurationCategory.GeneralInfo);

        public int Shutter3Type => this.GetIntegerConfigurationValue(GeneralInfo.Shutter3Type, ConfigurationCategory.GeneralInfo);

        public string Zip => this.GetStringConfigurationValue(GeneralInfo.Zip, ConfigurationCategory.GeneralInfo);

        #endregion
    }
}
