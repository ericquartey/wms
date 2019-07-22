using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService : IResolutionCalibration
    {
        #region Properties

        public Task<decimal> FeedRate => this.GetDecimalConfigurationValueAsync((long)ResolutionCalibration.FeedRate, (long)ConfigurationCategory.ResolutionCalibration);

        public Task<decimal> FinalPosition => this.GetDecimalConfigurationValueAsync((long)ResolutionCalibration.FinalPosition, (long)ConfigurationCategory.ResolutionCalibration);

        public Task<decimal> InitialPosition => this.GetDecimalConfigurationValueAsync((long)ResolutionCalibration.InitialPosition, (long)ConfigurationCategory.ResolutionCalibration);

        #endregion
    }
}
