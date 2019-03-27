namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IHorizontalMovementForwardProfile
    {
        #region Properties

        decimal InitialSpeed { get; }

        decimal Step1AccDec { get; }

        decimal Step1Position { get; }

        decimal Step1Speed { get; }

        decimal Step2AccDec { get; }

        decimal Step2Position { get; }

        decimal Step2Speed { get; }

        decimal Step3AccDec { get; }

        decimal Step3Position { get; }

        decimal Step3Speed { get; }

        decimal Step4AccDec { get; }

        decimal Step4Position { get; }

        decimal Step4Speed { get; }

        int TotalSteps { get; }

        #endregion
    }
}
