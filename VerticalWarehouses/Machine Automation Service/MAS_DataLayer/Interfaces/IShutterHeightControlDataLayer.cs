namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IShutterHeightControlDataLayer
    {
        #region Properties

        decimal FeedRateSH { get; }

        decimal RequiredTolerance { get; }

        #endregion
    }
}
