namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IWeightControl
    {
        #region Properties

        decimal FeedRateWC { get; }

        decimal RequiredToleranceWC { get; }

        decimal TestRun { get; }

        #endregion
    }
}
