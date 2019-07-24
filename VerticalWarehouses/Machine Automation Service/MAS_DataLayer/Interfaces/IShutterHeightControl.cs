namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IShutterHeightControl
    {
        #region Properties

        decimal FeedRateSH { get; }

        decimal RequiredTolerance { get; }

        #endregion
    }
}
