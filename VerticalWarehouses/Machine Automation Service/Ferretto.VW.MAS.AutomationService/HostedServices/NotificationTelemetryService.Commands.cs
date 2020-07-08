using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationTelemetryService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            // do not accept any commands
            return false;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
