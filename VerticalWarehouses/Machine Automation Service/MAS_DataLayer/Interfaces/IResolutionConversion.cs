using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IResolutionConversionDataLayer
    {
        #region Methods

        Task<int> MeterSUToPulsesConversion(decimal milliMeters, ConfigurationCategory configurationCategory);

        Task<decimal> PulsesToMeterSUConversion(int pulses, ConfigurationCategory configurationCategory);

        #endregion
    }
}
