using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces
{
    public interface ITemplateStateData : IStateData
    {


        #region Properties

        string Message { get; }

        #endregion
    }
}
