using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IBayPositionControlDataLayer
    {
        #region Properties

        public decimal FeedRateBP => this.GetDecimalConfigurationValue(BayPositionControl.FeedRate, ConfigurationCategory.BayPositionControl);

        public decimal StepValueBP => this.GetDecimalConfigurationValue(BayPositionControl.StepValue, ConfigurationCategory.BayPositionControl);

        #endregion
    }
}
