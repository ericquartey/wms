namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IHorizontalMovementBackwardProfile
    {
        #region Properties

        decimal InitialSpeedBP { get; }

        decimal Step1AccDecBP { get; }

        decimal Step1PositionBP { get; }

        decimal Step1SpeedBP { get; }

        decimal Step2AccDecBP { get; }

        decimal Step2PositionBP { get; }

        decimal Step2SpeedBP { get; }

        decimal Step3AccDecBP { get; }

        decimal Step3PositionBP { get; }

        decimal Step3SpeedBP { get; }

        decimal Step4AccDecBP { get; }

        decimal Step4PositionBP { get; }

        decimal Step4SpeedBP { get; }

        int TotalSteps { get; }

        #endregion
    }
}
