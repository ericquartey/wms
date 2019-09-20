using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces
{
    internal interface ITemplateMachineData : IMachineData
    {
        #region Properties

        string Message { get; }

        #endregion
    }
}
