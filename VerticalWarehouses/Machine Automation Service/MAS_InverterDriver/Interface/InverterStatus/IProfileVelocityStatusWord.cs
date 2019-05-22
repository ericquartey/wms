namespace Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus
{
    public interface IProfileVelocityStatusWord : IStatusWord
    {
        #region Properties

        bool TargetReached { get; }

        #endregion
    }
}
