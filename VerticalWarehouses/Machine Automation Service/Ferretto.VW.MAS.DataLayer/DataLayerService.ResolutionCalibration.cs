using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IResolutionCalibrationDataLayer
    {
        #region Properties

        public decimal FeedRate => this.GetDecimalConfigurationValue(ResolutionCalibration.FeedRate, ConfigurationCategory.ResolutionCalibration);

        public decimal FinalPosition => this.GetDecimalConfigurationValue(ResolutionCalibration.FinalPosition, ConfigurationCategory.ResolutionCalibration);

        public decimal InitialPosition => this.GetDecimalConfigurationValue(ResolutionCalibration.InitialPosition, ConfigurationCategory.ResolutionCalibration);

        #endregion
    }
}
