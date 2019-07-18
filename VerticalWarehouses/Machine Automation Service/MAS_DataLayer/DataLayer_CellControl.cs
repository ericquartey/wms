using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : ICellControl
    {
        #region Properties

        public Task<decimal> FeedRateCC => this.GetDecimalConfigurationValueAsync((long)CellControl.FeedRate, (long)ConfigurationCategory.CellControl);

        public Task<decimal> StepValueCC => this.GetDecimalConfigurationValueAsync((long)CellControl.StepValue, (long)ConfigurationCategory.CellControl);

        #endregion
    }
}
