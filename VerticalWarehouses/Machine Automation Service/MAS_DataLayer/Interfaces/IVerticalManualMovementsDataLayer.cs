namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVerticalManualMovementsDataLayer
    {
        #region Properties

        decimal FeedRateVM { get; }

        decimal InitialTargetPositionVM { get; }

        decimal RecoveryTargetPositionVM { get; }

        #endregion
    }
}
