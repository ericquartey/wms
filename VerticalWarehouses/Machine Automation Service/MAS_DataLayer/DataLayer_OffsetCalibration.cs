using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IOffsetCalibration
    {
        #region Properties

        public decimal FeedRateOC => this.GetDecimalConfigurationValue((long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);

        public int ReferenceCell => this.GetIntegerConfigurationValue((long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

        public decimal StepValue => this.GetDecimalConfigurationValue((long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

        #endregion
    }
}
