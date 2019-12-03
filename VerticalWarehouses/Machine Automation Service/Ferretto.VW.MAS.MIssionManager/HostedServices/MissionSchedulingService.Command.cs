using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            Contract.Requires(command != null);

            return
                command.Destination is MessageActor.Any
                ||
                command.Destination is MessageActor.MissionManager;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            // do nothing
            return Task.CompletedTask;
        }

        #endregion
    }
}
