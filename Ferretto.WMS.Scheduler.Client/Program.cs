using System;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WebAPI;

namespace Ferretto.WMS.Scheduler.Client
{
    internal class Program
    {
        #region Methods

        private static async Task Main(string[] args)
        {
            var machinesClient1 = new MachinesClient("http://localhost:5000/");
            var machinesClient2 = new MachinesClient("http://localhost:5000/");

            var machines = await machinesClient1.GetAllAsync();
            Console.WriteLine($"Machines Retrieved (machine[0] is {machines[0].AisleName}).");

            Console.WriteLine("Presse <Enter> to exit.");
            Console.ReadKey();
        }

        #endregion Methods
    }
}
