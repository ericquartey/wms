using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IPanelControlDataLayer
    {
        #region Properties

        public Task<int> BackInitialReferenceCell => this.GetIntegerConfigurationValueAsync((long)PanelControl.BackInitialReferenceCell, (long)ConfigurationCategory.PanelControl);

        public Task<int> BackPanelQuantity => this.GetIntegerConfigurationValueAsync((long)PanelControl.BackPanelQuantity, (long)ConfigurationCategory.PanelControl);

        public Task<decimal> FeedRatePC => this.GetDecimalConfigurationValueAsync((long)PanelControl.FeedRate, (long)ConfigurationCategory.PanelControl);

        public Task<int> FrontInitialReferenceCell => this.GetIntegerConfigurationValueAsync((long)PanelControl.FrontInitialReferenceCell, (long)ConfigurationCategory.PanelControl);

        public Task<int> FrontPanelQuantity => this.GetIntegerConfigurationValueAsync((long)PanelControl.FrontPanelQuantity, (long)ConfigurationCategory.PanelControl);

        public Task<decimal> StepValuePC => this.GetDecimalConfigurationValueAsync((long)PanelControl.StepValue, (long)ConfigurationCategory.PanelControl);

        #endregion
    }
}
