namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IProfileHeightCheckDataLayer
    {
        #region Properties

        decimal FeedRateSH { get; }

        decimal RequiredTolerance { get; }

        #endregion
    }
}
