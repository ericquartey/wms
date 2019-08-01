using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IBayPositionControlDataLayer
    {
		#region Properties

		public decimal FeedRateBP => this.GetDecimalConfigurationValue((long)BayPositionControl.FeedRate, (long)ConfigurationCategory.BayPositionControl);

		public decimal StepValueBP => this.GetDecimalConfigurationValue((long)BayPositionControl.StepValue, (long)ConfigurationCategory.BayPositionControl);

        #endregion
    }
}
