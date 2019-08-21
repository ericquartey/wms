using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IOffsetCalibrationDataLayer
    {
        #region Properties

        public decimal FeedRateOC => this.GetDecimalConfigurationValue((long)OffsetCalibration.FeedRate, ConfigurationCategory.OffsetCalibration);

        public int ReferenceCell => this.GetIntegerConfigurationValue((long)OffsetCalibration.ReferenceCell, ConfigurationCategory.OffsetCalibration);

        public decimal StepValue => this.GetDecimalConfigurationValue((long)OffsetCalibration.StepValue, ConfigurationCategory.OffsetCalibration);

        #endregion
    }
}
