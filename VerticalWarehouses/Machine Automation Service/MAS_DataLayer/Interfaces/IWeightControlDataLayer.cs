namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IWeightControlDataLayer
    {
        #region Properties

        decimal FeedRateWC { get; }

        decimal RequiredToleranceWC { get; }

        decimal TestRun { get; }

        #endregion
    }
}
