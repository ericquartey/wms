namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IResolutionCalibration
    {
        #region Properties

        decimal FeedRate { get; }

        decimal FinalPosition { get; }

        decimal ReferenceCellRC { get; }

        #endregion
    }
}
