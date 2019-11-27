using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed partial class WmsDataSyncService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            // do not accept any commands
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
