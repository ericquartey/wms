using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MissionManager
{
    internal partial class MissionManagerService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
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
