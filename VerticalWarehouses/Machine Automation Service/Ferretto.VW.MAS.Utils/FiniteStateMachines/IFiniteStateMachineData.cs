using System;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IFiniteStateMachineData
    {
        #region Properties

        Guid MachineId { get; }

        #endregion
    }
}
