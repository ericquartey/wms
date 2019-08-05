namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVerticalManualMovementsDataLayer
    {
        #region Properties

        decimal FeedRateAfterZero { get; }

        decimal FeedRateVM { get; }

        decimal NegativeTargetPosition { get; }

        decimal PositiveTargetPosition { get; }

        #endregion
    }
}
