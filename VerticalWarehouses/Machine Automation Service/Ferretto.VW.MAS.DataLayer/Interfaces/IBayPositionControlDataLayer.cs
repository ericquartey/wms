namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IBayPositionControlDataLayer
    {
        #region Properties
        decimal FeedRateBP { get; }
        decimal StepValueBP { get; }

        #endregion
    }
}
