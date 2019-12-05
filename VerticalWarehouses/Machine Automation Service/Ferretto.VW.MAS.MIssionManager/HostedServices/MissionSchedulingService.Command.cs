using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            // do not receive any commands
            return false;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            // do nothing
            return Task.CompletedTask;
        }

        #endregion
    }
}
