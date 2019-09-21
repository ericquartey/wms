using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;

namespace Ferretto.VW.MAS.AutomationService.StateMachines.Template.Interfaces
{
    public interface ITemplateStateData : IStateData
    {


        #region Properties

        string Message { get; }

        #endregion
    }
}
