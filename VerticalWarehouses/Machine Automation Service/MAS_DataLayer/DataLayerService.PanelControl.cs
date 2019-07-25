using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IPanelControl
    {
        #region Properties

        public int BackInitialReferenceCell => this.GetIntegerConfigurationValue((long)PanelControl.BackInitialReferenceCell, (long)ConfigurationCategory.PanelControl);

        public int BackPanelQuantity => this.GetIntegerConfigurationValue((long)PanelControl.BackPanelQuantity, (long)ConfigurationCategory.PanelControl);

        public decimal FeedRatePC => this.GetDecimalConfigurationValue((long)PanelControl.FeedRate, (long)ConfigurationCategory.PanelControl);

        public int FrontInitialReferenceCell => this.GetIntegerConfigurationValue((long)PanelControl.FrontInitialReferenceCell, (long)ConfigurationCategory.PanelControl);

        public int FrontPanelQuantity => this.GetIntegerConfigurationValue((long)PanelControl.FrontPanelQuantity, (long)ConfigurationCategory.PanelControl);

        public decimal StepValuePC => this.GetDecimalConfigurationValue((long)PanelControl.StepValue, (long)ConfigurationCategory.PanelControl);

        #endregion
    }
}
