using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.VW.MAS.AutomationService
{
    public class OperatorHub : Hub<IOperatorHub>
    {
    }
}
