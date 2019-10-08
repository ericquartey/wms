namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IShutterManualMovementsDataLayer
    {
        #region Properties

        decimal Acceleration { get; }

        decimal Deceleration { get; }

        decimal FeedRateSM { get; }

        decimal HigherDistance { get; }

        decimal HighSpeedDurationClose { get; }

        decimal HighSpeedDurationOpen { get; }

        decimal LowerDistance { get; }

        decimal MaxSpeed { get; }

        decimal MinSpeed { get; }

        #endregion
    }
}
