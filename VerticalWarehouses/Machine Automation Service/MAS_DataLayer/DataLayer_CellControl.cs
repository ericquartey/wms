using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ICellControl
    {
        #region Properties

        public decimal FeedRateCC => this.GetDecimalConfigurationValue((long)CellControl.FeedRate, (long)ConfigurationCategory.CellControl);

        public decimal StepValueCC => this.GetDecimalConfigurationValue((long)CellControl.StepValue, (long)ConfigurationCategory.CellControl);

        #endregion
    }
}
