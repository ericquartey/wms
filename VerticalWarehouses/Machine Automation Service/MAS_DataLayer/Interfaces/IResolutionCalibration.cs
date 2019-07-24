namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IResolutionCalibration
    {
        #region Properties

        decimal FeedRate { get; }

        decimal FinalPosition { get; }

        decimal InitialPosition { get; }

        #endregion
    }
}
