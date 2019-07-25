using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IResolutionConversion
    {
        #region Methods

        int MeterSUToPulsesConversion(decimal milliMeters, ConfigurationCategory configurationCategory);

        decimal PulsesToMeterSUConversion(int pulses, ConfigurationCategory configurationCategory);

        #endregion
    }
}
