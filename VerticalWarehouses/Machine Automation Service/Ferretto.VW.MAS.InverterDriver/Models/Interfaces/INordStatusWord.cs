using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus
{
    public interface INordStatusWord : IStatusWord
    {
        #region Properties

        bool Active481_10 { get; }

        bool Active481_9 { get; }

        bool BusControlActive { get; }

        bool ParameterSet1 { get; }

        bool ParameterSet2 { get; }

        bool RotationLeft { get; }

        bool RotationRight { get; }

        bool SetpointReached { get; }

        #endregion
    }
}
