using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IBayPositionControl
    {
        #region Properties

        public decimal StepValueBP => this.GetDecimalConfigurationValue((long)BayPositionControl.StepValue, (long)ConfigurationCategory.BayPositionControl);

        #endregion
    }
}
