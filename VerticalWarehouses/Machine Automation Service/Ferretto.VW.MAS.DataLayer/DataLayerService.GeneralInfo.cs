using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IGeneralInfoConfigurationDataLayer
    {


        #region Properties

        public string Address => this.GetStringConfigurationValue((long)GeneralInfo.Address, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay1 => this.GetBoolConfigurationValue((long)GeneralInfo.AlfaNumBay1, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay2 => this.GetBoolConfigurationValue((long)GeneralInfo.AlfaNumBay2, ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay3 => this.GetBoolConfigurationValue((long)GeneralInfo.AlfaNumBay3, ConfigurationCategory.GeneralInfo);

        public int Barrier1Height => this.GetIntegerConfigurationValue((long)GeneralInfo.Barrier1Height, ConfigurationCategory.GeneralInfo);

        public int Barrier2Height => this.GetIntegerConfigurationValue((long)GeneralInfo.Barrier2Height, ConfigurationCategory.GeneralInfo);

        public int Barrier3Height => this.GetIntegerConfigurationValue((long)GeneralInfo.Barrier3Height, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Height1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Height1, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Height2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Height2, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Position1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Position1, ConfigurationCategory.GeneralInfo);

        public decimal Bay1Position2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Position2, ConfigurationCategory.GeneralInfo);

        public int Bay1Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Bay1Type, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Height1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Height1, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Height2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Height2, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Position1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Position1, ConfigurationCategory.GeneralInfo);

        public decimal Bay2Position2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Position2, ConfigurationCategory.GeneralInfo);

        public int Bay2Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Bay2Type, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Height1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Height1, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Height2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Height2, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Position1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Position1, ConfigurationCategory.GeneralInfo);

        public decimal Bay3Position2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Position2, ConfigurationCategory.GeneralInfo);

        public int Bay3Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Bay3Type, ConfigurationCategory.GeneralInfo);

        public int BaysQuantity => this.GetIntegerConfigurationValue((long)GeneralInfo.BaysQuantity, ConfigurationCategory.GeneralInfo);

        public string City => this.GetStringConfigurationValue((long)GeneralInfo.City, ConfigurationCategory.GeneralInfo);

        public string ClientCode => this.GetStringConfigurationValue((long)GeneralInfo.ClientCode, ConfigurationCategory.GeneralInfo);

        public string ClientName => this.GetStringConfigurationValue((long)GeneralInfo.ClientName, ConfigurationCategory.GeneralInfo);

        public string Country => this.GetStringConfigurationValue((long)GeneralInfo.Country, ConfigurationCategory.GeneralInfo);

        public int DrawersQuantity => this.GetIntegerConfigurationValue((long)GeneralInfo.DrawersQuantity, ConfigurationCategory.GeneralInfo);

        public decimal Height => this.GetDecimalConfigurationValue((long)GeneralInfo.Height, ConfigurationCategory.GeneralInfo);

        public DateTime InstallationDate => this.GetDateTimeConfigurationValue((long)GeneralInfo.InstallationDate, ConfigurationCategory.GeneralInfo);

        public bool LaserBay1 => this.GetBoolConfigurationValue((long)GeneralInfo.LaserBay1, ConfigurationCategory.GeneralInfo);

        public bool LaserBay2 => this.GetBoolConfigurationValue((long)GeneralInfo.LaserBay2, ConfigurationCategory.GeneralInfo);

        public bool LaserBay3 => this.GetBoolConfigurationValue((long)GeneralInfo.LaserBay3, ConfigurationCategory.GeneralInfo);

        public string Latitude => this.GetStringConfigurationValue((long)GeneralInfo.Latitude, ConfigurationCategory.GeneralInfo);

        public string Longitude => this.GetStringConfigurationValue((long)GeneralInfo.Longitude, ConfigurationCategory.GeneralInfo);

        public int MaxAcceptedBai1Height => this.GetIntegerConfigurationValue((long)GeneralInfo.MaxAcceptedBai1Height, ConfigurationCategory.GeneralInfo);

        public int MaxAcceptedBai2Height => this.GetIntegerConfigurationValue((long)GeneralInfo.MaxAcceptedBai2Height, ConfigurationCategory.GeneralInfo);

        public int MaxAcceptedBai3Height => this.GetIntegerConfigurationValue((long)GeneralInfo.MaxAcceptedBai3Height, ConfigurationCategory.GeneralInfo);

        public int MaxDrawerGrossWeight => this.GetIntegerConfigurationValue((long)GeneralInfo.MaxDrawerGrossWeight, ConfigurationCategory.GeneralInfo);

        public double MaxGrossWeight => this.GetIntegerConfigurationValue((long)GeneralInfo.MaxGrossWeight, ConfigurationCategory.GeneralInfo);

        public string Model => this.GetStringConfigurationValue((long)GeneralInfo.Model, ConfigurationCategory.GeneralInfo);

        public string Order => this.GetStringConfigurationValue((long)GeneralInfo.Order, ConfigurationCategory.GeneralInfo);

        public DateTime ProductionDate => this.GetDateTimeConfigurationValue((long)GeneralInfo.ProductionDate, ConfigurationCategory.GeneralInfo);

        public string Province => this.GetStringConfigurationValue((long)GeneralInfo.Province, ConfigurationCategory.GeneralInfo);

        public string Serial => this.GetStringConfigurationValue((long)GeneralInfo.Serial, ConfigurationCategory.GeneralInfo);

        public int Shutter1Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Shutter1Type, ConfigurationCategory.GeneralInfo);

        public int Shutter2Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Shutter2Type, ConfigurationCategory.GeneralInfo);

        public int Shutter3Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Shutter3Type, ConfigurationCategory.GeneralInfo);

        public string Zip => this.GetStringConfigurationValue((long)GeneralInfo.Zip, ConfigurationCategory.GeneralInfo);

        #endregion
    }
}
