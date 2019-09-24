using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    public class OperatorHub : Hub<IOperatorHub>
    {
    }
}
