using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces
{
    public interface IPowerEnableMachineData : IMachineData
    {


        #region Properties

        bool Enable { get; }

        #endregion
    }
}
