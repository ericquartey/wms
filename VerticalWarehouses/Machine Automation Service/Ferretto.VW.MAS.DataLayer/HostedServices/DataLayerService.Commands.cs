using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return true;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command, IServiceProvider serviceProvider)
        {
            serviceProvider
                .GetRequiredService<ILogEntriesProvider>()
                .Add(command);

            return Task.CompletedTask;
        }

        #endregion
    }
}
