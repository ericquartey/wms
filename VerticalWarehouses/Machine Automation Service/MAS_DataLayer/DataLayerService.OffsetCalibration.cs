using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IOffsetCalibrationDataLayer
    {
        #region Properties

        public Task<decimal> FeedRateOC => this.GetDecimalConfigurationValueAsync((long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);

        public Task<int> ReferenceCell => this.GetIntegerConfigurationValueAsync((long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

        public Task<decimal> StepValue => this.GetDecimalConfigurationValueAsync((long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

        #endregion
    }
}
