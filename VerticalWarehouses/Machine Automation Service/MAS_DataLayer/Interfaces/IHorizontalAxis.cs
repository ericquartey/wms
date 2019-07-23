namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IHorizontalAxis
    {
        #region Properties

        decimal AntiClockWiseRun { get; }

        decimal ClockWiseRun { get; }

        bool HomingExecutedHA { get; }

        decimal MaxAccelerationHA { get; }

        decimal MaxDecelerationHA { get; }

        decimal MaxSpeedHA { get; }

        decimal OffsetHA { get; }

        decimal ResolutionHA { get; }

        #endregion
    }
}
