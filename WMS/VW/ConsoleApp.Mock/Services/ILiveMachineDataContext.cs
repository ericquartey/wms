using Ferretto.VW.AutomationService.Hubs;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public interface ILiveMachineDataContext
    {
        #region Properties

        MachineStatus MachineStatus { get; }

        #endregion
    }
}
