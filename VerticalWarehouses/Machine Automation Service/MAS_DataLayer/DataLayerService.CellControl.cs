using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ICellControlDataLayer
    {
        #region Properties

        public decimal FeedRateCC => this.GetDecimalConfigurationValue((long)CellControl.FeedRate, (long)ConfigurationCategory.CellControl);

        public decimal StepValueCC => this.GetDecimalConfigurationValue((long)CellControl.StepValue, (long)ConfigurationCategory.CellControl);

        #endregion
    }
}
