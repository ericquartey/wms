using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IResolutionCalibration
    {
        #region Properties

        public decimal FeedRate => this.GetDecimalConfigurationValue((long)ResolutionCalibration.FeedRate, (long)ConfigurationCategory.ResolutionCalibration);

        public decimal FinalPosition => this.GetDecimalConfigurationValue((long)ResolutionCalibration.FinalPosition, (long)ConfigurationCategory.ResolutionCalibration);

        public decimal ReferenceCellRC => this.GetDecimalConfigurationValue((long)ResolutionCalibration.ReferenceCell, (long)ConfigurationCategory.ResolutionCalibration);

        #endregion
    }
}
