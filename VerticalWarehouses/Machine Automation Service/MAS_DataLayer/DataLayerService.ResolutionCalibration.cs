using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IResolutionCalibration
    {
        #region Properties

        public decimal FeedRate => this.GetDecimalConfigurationValue((long)ResolutionCalibration.FeedRate, (long)ConfigurationCategory.ResolutionCalibration);

        public decimal FinalPosition => this.GetDecimalConfigurationValue((long)ResolutionCalibration.FinalPosition, (long)ConfigurationCategory.ResolutionCalibration);

        public decimal InitialPosition => this.GetDecimalConfigurationValue((long)ResolutionCalibration.InitialPosition, (long)ConfigurationCategory.ResolutionCalibration);

        #endregion
    }
}
