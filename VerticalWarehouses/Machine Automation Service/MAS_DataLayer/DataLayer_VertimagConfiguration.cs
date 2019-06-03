using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IVertimagConfiguration
    {
        #region Methods

        public async Task<Dictionary<InverterIndex, InverterType>> GetInstalledInverterListAsync()
        {
            Dictionary<InverterIndex, InverterType> installedInverters = new Dictionary<InverterIndex, InverterType>
            {
                {InverterIndex.MainInverter, InverterType.Ang},
                {InverterIndex.Slave1, InverterType.Agl },
            };

            await Task.Delay(5, this.stoppingToken);

            return installedInverters;
        }

        #endregion
    }
}
