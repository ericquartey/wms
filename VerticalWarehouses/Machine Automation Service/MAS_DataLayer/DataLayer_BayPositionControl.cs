using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : IBayPositionControl
    {
        #region Properties

        public Task<decimal> StepValueBP => this.GetDecimalConfigurationValueAsync((long)BayPositionControl.StepValue, (long)ConfigurationCategory.BayPositionControl);

        #endregion
    }
}
