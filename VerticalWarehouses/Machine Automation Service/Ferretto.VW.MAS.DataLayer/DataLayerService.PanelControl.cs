using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IPanelControlDataLayer
    {
        #region Properties

        public int BackInitialReferenceCell => this.GetIntegerConfigurationValue((long)PanelControl.BackInitialReferenceCell, ConfigurationCategory.PanelControl);

        public int BackPanelQuantity => this.GetIntegerConfigurationValue((long)PanelControl.BackPanelQuantity, ConfigurationCategory.PanelControl);

        public decimal FeedRatePC => this.GetDecimalConfigurationValue((long)PanelControl.FeedRate, ConfigurationCategory.PanelControl);

        public int FrontInitialReferenceCell => this.GetIntegerConfigurationValue((long)PanelControl.FrontInitialReferenceCell, ConfigurationCategory.PanelControl);

        public int FrontPanelQuantity => this.GetIntegerConfigurationValue((long)PanelControl.FrontPanelQuantity, ConfigurationCategory.PanelControl);

        public decimal StepValuePC => this.GetDecimalConfigurationValue((long)PanelControl.StepValue, ConfigurationCategory.PanelControl);

        #endregion
    }
}
