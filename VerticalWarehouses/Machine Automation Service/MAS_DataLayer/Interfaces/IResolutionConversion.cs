using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IResolutionConversion
    {
        #region Methods

        Task<int> MilliMetersToPulsesConversion(decimal milliMeters, ConfigurationCategory configurationCategory);

        Task<decimal> PulsesToMilliMetersConversion(int pulses, ConfigurationCategory configurationCategory);

        #endregion
    }
}
