using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IGeneralInfoConfigurationDataLayer
    {
        #region Properties

        public Task<string> Address => this.GetStringConfigurationValueAsync((long)GeneralInfo.Address, (long)ConfigurationCategory.GeneralInfo);

        public Task<bool> AlfaNumBay1 => this.GetBoolConfigurationValueAsync((long)GeneralInfo.AlfaNumBay1, (long)ConfigurationCategory.GeneralInfo);

        public Task<bool> AlfaNumBay2 => this.GetBoolConfigurationValueAsync((long)GeneralInfo.AlfaNumBay2, (long)ConfigurationCategory.GeneralInfo);

        public Task<bool> AlfaNumBay3 => this.GetBoolConfigurationValueAsync((long)GeneralInfo.AlfaNumBay3, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Barrier1Height => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Barrier1Height, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Barrier2Height => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Barrier2Height, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Barrier3Height => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Barrier3Height, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay1Height1 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay1Height1, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay1Height2 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay1Height2, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay1Position1 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay1Position1, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay1Position2 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay1Position2, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Bay1Type => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Bay1Type, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay2Height1 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay2Height1, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay2Height2 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay2Height2, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay2Position1 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay2Position1, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay2Position2 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay2Position2, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Bay2Type => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Bay2Type, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay3Height1 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay3Height1, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay3Height2 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay3Height2, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay3Position1 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay3Position1, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Bay3Position2 => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Bay3Position2, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Bay3Type => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Bay3Type, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> BaysQuantity => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.BaysQuantity, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> City => this.GetStringConfigurationValueAsync((long)GeneralInfo.City, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> ClientCode => this.GetStringConfigurationValueAsync((long)GeneralInfo.ClientCode, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> ClientName => this.GetStringConfigurationValueAsync((long)GeneralInfo.ClientName, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> Country => this.GetStringConfigurationValueAsync((long)GeneralInfo.Country, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> DrawersQuantity => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.DrawersQuantity, (long)ConfigurationCategory.GeneralInfo);

        public Task<decimal> Height => this.GetDecimalConfigurationValueAsync((long)GeneralInfo.Height, (long)ConfigurationCategory.GeneralInfo);

        public Task<bool> LaserBay1 => this.GetBoolConfigurationValueAsync((long)GeneralInfo.LaserBay1, (long)ConfigurationCategory.GeneralInfo);

        public Task<bool> LaserBay2 => this.GetBoolConfigurationValueAsync((long)GeneralInfo.LaserBay2, (long)ConfigurationCategory.GeneralInfo);

        public Task<bool> LaserBay3 => this.GetBoolConfigurationValueAsync((long)GeneralInfo.LaserBay3, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> Latitude => this.GetStringConfigurationValueAsync((long)GeneralInfo.Latitude, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> Longitude => this.GetStringConfigurationValueAsync((long)GeneralInfo.Longitude, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> MaxAcceptedBai1Height => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.MaxAcceptedBai1Height, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> MaxAcceptedBai2Height => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.MaxAcceptedBai2Height, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> MaxAcceptedBai3Height => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.MaxAcceptedBai3Height, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> Order => this.GetStringConfigurationValueAsync((long)GeneralInfo.Order, (long)ConfigurationCategory.GeneralInfo);

        public Task<DateTime> ProductionDate => this.GetDateTimeConfigurationValueAsync((long)GeneralInfo.ProductionDate, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> Province => this.GetStringConfigurationValueAsync((long)GeneralInfo.Province, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Shutter1Type => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter1Type, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Shutter2Type => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter2Type, (long)ConfigurationCategory.GeneralInfo);

        public Task<int> Shutter3Type => this.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter3Type, (long)ConfigurationCategory.GeneralInfo);

        public Task<string> Zip => this.GetStringConfigurationValueAsync((long)GeneralInfo.Zip, (long)ConfigurationCategory.GeneralInfo);

        #endregion

        #region Methods

        public async Task<DateTime> GetInstallationDate() => await this.GetDateTimeConfigurationValueAsync((long)GeneralInfo.InstallationDate, (long)ConfigurationCategory.GeneralInfo);

        public async Task<decimal> GetMaxGrossWeight() => await this.GetDecimalConfigurationValueAsync((long)GeneralInfo.MaxGrossWeight, (long)ConfigurationCategory.GeneralInfo);

        public async Task<string> GetModel() => await this.GetStringConfigurationValueAsync((long)GeneralInfo.Model, (long)ConfigurationCategory.GeneralInfo);

        public async Task<string> GetSerial() => await this.GetStringConfigurationValueAsync((long)GeneralInfo.Serial, (long)ConfigurationCategory.GeneralInfo);

        #endregion
    }
}
