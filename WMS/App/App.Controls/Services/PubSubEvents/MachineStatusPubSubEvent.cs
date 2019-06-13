using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.App.Controls.Services
{
    public class MachineStatusPubSubEvent : Prism.Events.PubSubEvent, IPubSubEvent
    {
        #region Constructors

        public MachineStatusPubSubEvent(VW.MachineAutomationService.Hubs.MachineStatus machineStatus)
        {
            this.MachineStatus = machineStatus;
        }

        #endregion

        #region Properties

        public Data.Hubs.Models.MachineStatus MachineStatus { get; }

        public string Token { get; }

        #endregion
    }
}
