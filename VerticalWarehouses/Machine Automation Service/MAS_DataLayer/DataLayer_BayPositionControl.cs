using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IBayPositionControlDataLayer
    {
        #region Properties

        public Task<decimal> StepValueBP => this.GetDecimalConfigurationValueAsync((long)BayPositionControl.StepValue, (long)ConfigurationCategory.BayPositionControl);

        #endregion
    }
}
