using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IResolutionCalibrationDataLayer
    {
        #region Properties

        public decimal FeedRate => this.GetDecimalConfigurationValue((long)ResolutionCalibration.FeedRate, (long)ConfigurationCategory.ResolutionCalibration);

        public decimal FinalPosition => this.GetDecimalConfigurationValue((long)ResolutionCalibration.FinalPosition, (long)ConfigurationCategory.ResolutionCalibration);

        public decimal InitialPosition => this.GetDecimalConfigurationValue((long)ResolutionCalibration.InitialPosition, (long)ConfigurationCategory.ResolutionCalibration);

        #endregion
    }
}
