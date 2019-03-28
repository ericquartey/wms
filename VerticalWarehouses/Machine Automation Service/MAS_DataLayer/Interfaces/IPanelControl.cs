namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IPanelControl
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
