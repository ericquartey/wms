using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayer : ICellControl
    {
        #region Properties

        public decimal FeedRateCC => this.GetDecimalConfigurationValue((long)CellControl.FeedRate, (long)ConfigurationCategory.CellControl);

        public decimal StepValueCC => this.GetDecimalConfigurationValue((long)CellControl.StepValue, (long)ConfigurationCategory.CellControl);

        #endregion
    }
}
