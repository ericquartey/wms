using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IResolutionCalibration
    {
        #region Properties

        public Task<decimal> FeedRate => this.GetDecimalConfigurationValueAsync((long)ResolutionCalibration.FeedRate, (long)ConfigurationCategory.ResolutionCalibration);

        public Task<decimal> FinalPosition => this.GetDecimalConfigurationValueAsync((long)ResolutionCalibration.FinalPosition, (long)ConfigurationCategory.ResolutionCalibration);

        public Task<decimal> InitialPosition => this.GetDecimalConfigurationValueAsync((long)ResolutionCalibration.InitialPosition, (long)ConfigurationCategory.ResolutionCalibration);

        #endregion
    }
}
