using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WCF.Client.Proxy;

namespace Ferretto.WMS.Scheduler.WCF.Client
{
    public class MachineCallbackHandler : IMachineCallback
    {
        public void WakeUpClients(String message)
        {
            Console.WriteLine("The server woke me up: " + message);
        }
    }
}
