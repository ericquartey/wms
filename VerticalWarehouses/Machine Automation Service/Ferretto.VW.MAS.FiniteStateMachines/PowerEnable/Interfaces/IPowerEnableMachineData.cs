using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces
{
    internal interface IPowerEnableMachineData : IMachineData
    {


        #region Properties

        bool Enable { get; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        #endregion
    }
}
