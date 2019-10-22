using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MissionManager.BackgroundService
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
            return Task.CompletedTask;
        }

        #endregion
    }
}
