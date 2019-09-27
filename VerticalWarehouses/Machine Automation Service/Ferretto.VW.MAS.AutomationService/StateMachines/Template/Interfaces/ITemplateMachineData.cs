using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;

namespace Ferretto.VW.MAS.AutomationService.StateMachines.Template.Interfaces
{
    public interface ITemplateMachineData : IMachineData
    {
        #region Properties

        string Message { get; }

        #endregion
    }
}
