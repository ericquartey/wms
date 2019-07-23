using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
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
