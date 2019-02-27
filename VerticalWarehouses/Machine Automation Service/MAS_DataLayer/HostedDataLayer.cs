using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.MAS_DataLayer
{
    public class HostedDataLayer : BackgroundService
    {
        #region Methods

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
