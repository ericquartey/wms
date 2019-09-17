using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.AutomationService
{
    partial class AutomationService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return
                command.Destination == MessageActor.AutomationService
                ||
                command.Destination == MessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command)
        {
            // do nothing
            return Task.CompletedTask;
        }

        #endregion
    }
}
