namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IHorizontalManualMovements
    {
        #region Properties

        decimal FeedRateHM { get; }

        decimal InitialTargetPositionHM { get; }

        decimal RecoveryTargetPositionHM { get; }

        #endregion
    }
}
