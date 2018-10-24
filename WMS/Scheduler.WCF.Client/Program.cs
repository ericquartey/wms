using System;
using System.ServiceModel;
using Ferretto.WMS.Scheduler.WCF.Client.Proxy;

namespace Ferretto.WMS.Scheduler.WCF.Client
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Console.WriteLine($"Press <ENTER> to start the application.");
            Console.ReadKey();

            var instanceContext = new InstanceContext(new MachineCallbackHandler());
            var machineClient = new MachineClient(instanceContext);

            var checkpoint1 = DateTime.Now;
            var machines = machineClient.GetAll();
            var checkpoint2 = DateTime.Now;

            Console.WriteLine($"[{checkpoint2 - checkpoint1}] Hello World! The first machine is {machines[0].AisleName}.");

            var result = machineClient.CompleteMission(1, 2);

            Console.WriteLine($"Press <ENTER> to close the application.");
            Console.ReadKey();
        }

        #endregion Methods
    }
}
