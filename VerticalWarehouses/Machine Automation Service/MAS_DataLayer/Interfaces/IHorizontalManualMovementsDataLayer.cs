namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalManualMovementsDataLayer
    {
        #region Properties

        decimal FeedRateHM { get; }

        decimal InitialTargetPositionHM { get; }

        decimal RecoveryTargetPositionHM { get; }

        #endregion
    }
}
