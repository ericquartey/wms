namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IVerticalAxis
    {
        #region Properties

        decimal DepositOffset { get; }

        bool HomingExecuted { get; }

        decimal HomingExitAcceleration { get; }

        decimal HomingExitDeceleration { get; }

        decimal HomingExitSpeed { get; }

        decimal HomingSearchAcceleration { get; }

        decimal HomingSearchDeceleration { get; }

        bool HomingSearchDirection { get; }

        decimal HomingSearchSpeed { get; }

        decimal LowerBound { get; }

        decimal MaxAcceleration { get; }

        decimal MaxDeceleration { get; }

        decimal MaxSpeed { get; }

        decimal Offset { get; }

        decimal Resolution { get; }

        decimal TakingOffset { get; }

        decimal UpperBound { get; }

        #endregion
    }
}
