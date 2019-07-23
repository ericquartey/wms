using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IOffsetCalibration
    {
        #region Properties

        public decimal FeedRateOC => this.GetDecimalConfigurationValue((long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);

        public int ReferenceCell => this.GetIntegerConfigurationValue((long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

        public decimal StepValue => this.GetDecimalConfigurationValue((long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

        #endregion
    }
}
