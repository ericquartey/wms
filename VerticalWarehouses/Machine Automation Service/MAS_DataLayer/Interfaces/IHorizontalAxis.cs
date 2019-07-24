namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalAxis
    {
        #region Properties

        decimal AntiClockWiseRun { get; }

        decimal ClockWiseRun { get; }

        bool HomingExecutedHA { get; }

        decimal MaxEmptyAccelerationHA { get; }

        decimal MaxEmptyDecelerationHA { get; }

        decimal MaxEmptySpeedHA { get; }

        decimal MaxFullAccelerationHA { get; }

        decimal MaxFullDecelerationHA { get; }

        decimal MaxFullSpeed { get; }

        decimal OffsetHA { get; }

        decimal ResolutionHA { get; }

        #endregion
    }
}
