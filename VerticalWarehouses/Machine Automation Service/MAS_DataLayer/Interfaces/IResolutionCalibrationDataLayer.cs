namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IResolutionCalibrationDataLayer
    {
        #region Properties

        decimal FeedRate { get; }

        decimal FinalPosition { get; }

        decimal InitialPosition { get; }

        #endregion
    }
}
