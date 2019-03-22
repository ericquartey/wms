//DataLayer_GeneralInfo.cs
using System;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interface;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IGeneralInfo
    {
        #region Properties

        public string Address => this.GetStringConfigurationValue((long)GeneralInfo.Address, (long)ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay1 => this.GetBoolConfigurationValue((long)GeneralInfo.AlfaNumBay1, (long)ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay2 => this.GetBoolConfigurationValue((long)GeneralInfo.AlfaNumBay2, (long)ConfigurationCategory.GeneralInfo);

        public bool AlfaNumBay3 => this.GetBoolConfigurationValue((long)GeneralInfo.AlfaNumBay3, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay1Height1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Height1, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay1Height2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Height2, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay1Position1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Position1, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay1Position2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay1Position2, (long)ConfigurationCategory.GeneralInfo);

        public int Bay1Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Bay1Type, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay2Height1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Height1, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay2Height2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Height2, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay2Position1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Position1, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay2Position2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay2Position2, (long)ConfigurationCategory.GeneralInfo);

        public int Bay2Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Bay2Type, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay3Height1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Height1, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay3Height2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Height2, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay3Position1 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Position1, (long)ConfigurationCategory.GeneralInfo);

        public decimal Bay3Position2 => this.GetDecimalConfigurationValue((long)GeneralInfo.Bay3Position2, (long)ConfigurationCategory.GeneralInfo);

        public int Bay3Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Bay3Type, (long)ConfigurationCategory.GeneralInfo);

        public int BaysQuantity => this.GetIntegerConfigurationValue((long)GeneralInfo.BaysQuantity, (long)ConfigurationCategory.GeneralInfo);

        public string City => this.GetStringConfigurationValue((long)GeneralInfo.City, (long)ConfigurationCategory.GeneralInfo);

        public string ClientCode => this.GetStringConfigurationValue((long)GeneralInfo.ClientCode, (long)ConfigurationCategory.GeneralInfo);

        public string ClientName => this.GetStringConfigurationValue((long)GeneralInfo.ClientName, (long)ConfigurationCategory.GeneralInfo);

        public string Country => this.GetStringConfigurationValue((long)GeneralInfo.Country, (long)ConfigurationCategory.GeneralInfo);

        public int DrawersQuantity => this.GetIntegerConfigurationValue((long)GeneralInfo.DrawersQuantity, (long)ConfigurationCategory.GeneralInfo);

        public decimal Height => this.GetDecimalConfigurationValue((long)GeneralInfo.Height, (long)ConfigurationCategory.GeneralInfo);

        public DateTime InstallationDate => this.GetDateTimeConfigurationValue((long)GeneralInfo.InstallationDate, (long)ConfigurationCategory.GeneralInfo);

        public bool LaserBay1 => this.GetBoolConfigurationValue((long)GeneralInfo.LaserBay1, (long)ConfigurationCategory.GeneralInfo);

        public bool LaserBay2 => this.GetBoolConfigurationValue((long)GeneralInfo.LaserBay2, (long)ConfigurationCategory.GeneralInfo);

        public bool LaserBay3 => this.GetBoolConfigurationValue((long)GeneralInfo.LaserBay3, (long)ConfigurationCategory.GeneralInfo);

        public string Latitude => this.GetStringConfigurationValue((long)GeneralInfo.Latitude, (long)ConfigurationCategory.GeneralInfo);

        public string Longitude => this.GetStringConfigurationValue((long)GeneralInfo.Longitude, (long)ConfigurationCategory.GeneralInfo);

        public decimal MaxWeight => this.GetDecimalConfigurationValue((long)GeneralInfo.MaxWeight, (long)ConfigurationCategory.GeneralInfo);

        public string Model => this.GetStringConfigurationValue((long)GeneralInfo.Model, (long)ConfigurationCategory.GeneralInfo);

        public string Order => this.GetStringConfigurationValue((long)GeneralInfo.Order, (long)ConfigurationCategory.GeneralInfo);

        public DateTime ProductionDate => this.GetDateTimeConfigurationValue((long)GeneralInfo.ProductionDate, (long)ConfigurationCategory.GeneralInfo);

        public string Province => this.GetStringConfigurationValue((long)GeneralInfo.Province, (long)ConfigurationCategory.GeneralInfo);

        public string Serial => this.GetStringConfigurationValue((long)GeneralInfo.Serial, (long)ConfigurationCategory.GeneralInfo);

        public int Shutter1Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Shutter1Type, (long)ConfigurationCategory.GeneralInfo);

        public int Shutter2Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Shutter2Type, (long)ConfigurationCategory.GeneralInfo);

        public int Shutter3Type => this.GetIntegerConfigurationValue((long)GeneralInfo.Shutter3Type, (long)ConfigurationCategory.GeneralInfo);

        public string Zip => this.GetStringConfigurationValue((long)GeneralInfo.Zip, (long)ConfigurationCategory.GeneralInfo);

        #endregion
    }
}
