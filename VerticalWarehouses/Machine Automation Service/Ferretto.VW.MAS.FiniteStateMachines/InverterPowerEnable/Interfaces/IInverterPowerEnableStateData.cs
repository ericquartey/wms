using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.InverterPowerEnable.Interfaces
{
    internal interface IInverterPowerEnableStateData : IStateData
    {
        #region Properties

        string Message { get; }

        #endregion
    }
}
