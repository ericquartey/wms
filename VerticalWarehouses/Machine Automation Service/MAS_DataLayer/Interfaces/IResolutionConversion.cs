using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IResolutionConversion
    {
        #region Methods

        Task<int> MeterSUToPulsesConversion(decimal milliMeters, ConfigurationCategory configurationCategory);

        Task<decimal> PulsesToMeterSUConversion(int pulses, ConfigurationCategory configurationCategory);

        #endregion
    }
}
