using System;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WebAPI;

namespace Ferretto.WMS.Scheduler.Client
{
    internal static class Program
    {
        #region Fields

        private const string serverUrl = "http://localhost:5000/";
        private const string wakeupHubPath = "wakeup-hub";

        private static DateTime checkPointHubStart;

        #endregion Fields

        #region Methods

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Press <Enter> to start execution.");
            Console.ReadLine();

            // Create two clients
            //
            var machinesClient1 = new WebAPI.Contracts.MachinesClient(serverUrl);
            var machinesClient2 = new WebAPI.Contracts.MachinesClient(serverUrl);

            Console.WriteLine("Client 1 - Retrieving mahcines ...");
            var checkpoint1 = DateTime.Now;
            var machines = await machinesClient1.GetAllAsync();
            var checkpoint2 = DateTime.Now;
            Console.WriteLine($"[{checkpoint2 - checkpoint1}] Machines Retrieved (machine[0] is {machines[0].AisleName}).");

            // Listen to push messages
            //
            var wakeupHubClient1 = new WakeupHubClient($"{serverUrl}{wakeupHubPath}");
            wakeupHubClient1.WakeupReceived += WakeUpClient_WakeupReceived;
            await wakeupHubClient1.ConnectAsync();
            Console.WriteLine("Client 1 - Connection to SignalR server established.");

            var wakeupHubClient2 = new WakeupHubClient($"{serverUrl}{wakeupHubPath}");
            wakeupHubClient2.WakeupReceived += WakeupHubClient2_WakeupReceived;
            await wakeupHubClient2.ConnectAsync();
            Console.WriteLine("Client 2 - Connection to SignalR server established.");

            checkPointHubStart = DateTime.Now;
            wakeupHubClient2.NotifyServer();

            // Call GetAll again, so that the Wakeup message is sent through SignalR
            //
            Console.WriteLine("Client 1 - Retrieving mahcines ...");
            checkpoint1 = DateTime.Now;
            var machinesTake2 = await machinesClient1.GetAllAsync();
            checkpoint2 = DateTime.Now;
            Console.WriteLine($"[{checkpoint2 - checkpoint1}] Client 1 - Machines retrieved again.");

            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();
        }

        private static void WakeUpClient_WakeupReceived(Object sender, WebAPI.Contracts.WakeUpEventArgs e)
        {
            var checkPointHubEnd = DateTime.Now;
            Console.WriteLine($"[{checkPointHubEnd - Program.checkPointHubStart}] Client 1 - Message received from user {e.User}: {e.Message}");
        }

        private static void WakeupHubClient2_WakeupReceived(Object sender, WebAPI.Contracts.WakeUpEventArgs e)
        {
            var checkPointHubEnd = DateTime.Now;
            Console.WriteLine($"[{checkPointHubEnd - Program.checkPointHubStart}]Client 2 - Message received from user {e.User}: {e.Message}");
        }

        #endregion Methods
    }
}
