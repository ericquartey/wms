namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IShutterManualMovementsDataLayer
    {
        #region Properties

        decimal Acceleration { get; }

        decimal Deceleration { get; }

        decimal FeedRateSM { get; }

        decimal HigherDistance { get; }

        decimal LowerDistance { get; }

        decimal MaxSpeed { get; }

        decimal MinSpeed { get; }

        #endregion
    }
}
