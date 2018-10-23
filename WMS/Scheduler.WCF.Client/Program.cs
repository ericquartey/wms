using System;
using Ferretto.WMS.Scheduler.WCF.Client.Proxy;

namespace Ferretto.WMS.Scheduler.WCF.Client
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            var machinesClient = new CalculatorClient();
            var machines = machinesClient.GetAll();
           

            Console.WriteLine($"Hello World! The first machine is {machines[0].AisleName}.");


            var result = machinesClient.CompleteMission(1, 2);

            Console.WriteLine($"Press <ENTER> to close the application.");
            Console.ReadKey();
        }

        #endregion Methods
    }
}
