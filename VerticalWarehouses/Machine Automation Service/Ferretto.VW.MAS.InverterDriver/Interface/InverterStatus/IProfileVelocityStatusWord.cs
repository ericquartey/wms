using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface IProfileVelocityStatusWord : IStatusWord
    {
        #region Properties

        bool TargetReached { get; }

        #endregion
    }
}
