using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManagerService
    {


        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Destination == MessageActor.MissionsManager ||
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
