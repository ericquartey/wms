using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IPanelControlDataLayer
    {
        #region Properties

        public int BackInitialReferenceCell => this.GetIntegerConfigurationValue(PanelControl.BackInitialReferenceCell, ConfigurationCategory.PanelControl);

        public int BackPanelQuantity => this.GetIntegerConfigurationValue(PanelControl.BackPanelQuantity, ConfigurationCategory.PanelControl);

        public decimal FeedRatePC => this.GetDecimalConfigurationValue(PanelControl.FeedRate, ConfigurationCategory.PanelControl);

        public int FrontInitialReferenceCell => this.GetIntegerConfigurationValue(PanelControl.FrontInitialReferenceCell, ConfigurationCategory.PanelControl);

        public int FrontPanelQuantity => this.GetIntegerConfigurationValue(PanelControl.FrontPanelQuantity, ConfigurationCategory.PanelControl);

        public decimal StepValuePC => this.GetDecimalConfigurationValue(PanelControl.StepValue, ConfigurationCategory.PanelControl);

        #endregion
    }
}
