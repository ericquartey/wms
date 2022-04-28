using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.NordDriver
{
    public interface INordStatusWord : IStatusWord
    {
        #region Properties

        bool IsOperationEnabledNord { get; }

        bool IsQuickStopTrueNord { get; }

        bool ParameterSet1 { get; }

        bool ParameterSet2 { get; }

        bool TargetReached { get; }

        #endregion
    }
}
