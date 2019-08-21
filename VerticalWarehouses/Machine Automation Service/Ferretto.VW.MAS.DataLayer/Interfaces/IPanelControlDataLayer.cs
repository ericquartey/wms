namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IPanelControlDataLayer
    {
        #region Properties

        int BackInitialReferenceCell { get; }

        int BackPanelQuantity { get; }

        decimal FeedRatePC { get; }

        int FrontInitialReferenceCell { get; }

        int FrontPanelQuantity { get; }

        decimal StepValuePC { get; }

        #endregion
    }
}
