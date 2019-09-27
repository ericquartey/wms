using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IOffsetCalibrationDataLayer
    {
        #region Properties

        public decimal FeedRateOC => this.GetDecimalConfigurationValue(OffsetCalibration.FeedRate, ConfigurationCategory.OffsetCalibration);

        public int ReferenceCell => this.GetIntegerConfigurationValue(OffsetCalibration.ReferenceCell, ConfigurationCategory.OffsetCalibration);

        public decimal StepValue => this.GetDecimalConfigurationValue(OffsetCalibration.StepValue, ConfigurationCategory.OffsetCalibration);

        #endregion
    }
}
