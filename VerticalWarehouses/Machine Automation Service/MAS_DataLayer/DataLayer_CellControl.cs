using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ICellControlDataLayer
    {
        #region Properties

        public Task<decimal> FeedRateCC => this.GetDecimalConfigurationValueAsync((long)CellControl.FeedRate, (long)ConfigurationCategory.CellControl);

        public Task<decimal> StepValueCC => this.GetDecimalConfigurationValueAsync((long)CellControl.StepValue, (long)ConfigurationCategory.CellControl);

        #endregion
    }
}
