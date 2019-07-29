using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public partial class MachineServiceController
    {
        #region Methods

        private void ExecuteMachineReset_Method(ResetOperation operation)
        {
            var resetHardwareMessageData = new ResetHardwareMessageData(operation);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(resetHardwareMessageData, "Reset machine hardware", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ResetHardware));
        }

        #endregion
    }
}
