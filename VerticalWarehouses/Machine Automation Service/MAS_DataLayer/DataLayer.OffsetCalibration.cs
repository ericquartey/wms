using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IOffsetCalibration
    {
        #region Properties

        public Task<decimal> FeedRateOC => this.GetDecimalConfigurationValueAsync((long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);

        public Task<int> ReferenceCell => this.GetIntegerConfigurationValueAsync((long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

        public Task<decimal> StepValue => this.GetDecimalConfigurationValueAsync((long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

        #endregion
    }
}
