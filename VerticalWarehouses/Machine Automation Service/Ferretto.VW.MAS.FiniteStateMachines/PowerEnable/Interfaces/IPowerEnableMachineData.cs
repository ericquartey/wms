using System.Collections.Generic;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.PowerEnable.Interfaces
{
    public interface IPowerEnableMachineData : IMachineData
    {


        #region Properties

        List<InverterIndex> ConfiguredInverters { get; }

        List<IoIndex> ConfiguredIoDevices { get; }

        bool Enable { get; }

        #endregion
    }
}
