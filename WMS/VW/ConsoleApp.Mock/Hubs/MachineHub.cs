using Ferretto.VW.MachineAutomationService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public class MachineHub : Hub<IMachineHub>
    {
        #region Fields

        private readonly ILiveMachineDataContext liveMachineDataContext;

        #endregion

        #region Constructors

        public MachineHub(ILiveMachineDataContext liveMachineDataContext)
        {
            this.liveMachineDataContext = liveMachineDataContext;
        }

        #endregion

        #region Methods

        public void GetCurrentStatus()
        {
            this.Clients.Caller.EchoCurrentStatus(this.liveMachineDataContext.MachineStatus);
        }

        #endregion
    }
}
