using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces
{
    internal interface IMoveDrawerStateData : IStateData
    {
        #region Properties

        string Message { get; }

        #endregion
    }
}
