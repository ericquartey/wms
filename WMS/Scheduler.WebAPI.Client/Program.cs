using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.WebAPI.Contracts;

namespace Ferretto.WMS.Scheduler.WebAPI.Client
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

            //  await Prova1();
            //   await Prova2();
            await Prova3();

            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();
        }

        private static async Task Prova1()
        {
            var machinesClient = new MachinesClient(serverUrl);

            var machines = await machinesClient.GetAllAsync();
            Console.WriteLine($"Machines Retrieved (machine[0] is {machines[0].AisleName}).");
        }

        private static async Task Prova2()
        {
            // Create two clients
            //
            var machinesClient1 = new MachinesClient(serverUrl);

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
        }

        private static async Task Prova3()
        {
            var itemsClient = new ItemsClient(serverUrl);

            var sw = new System.Diagnostics.Stopwatch();

            IEnumerable<Item> items = null;
            for (var i = 0; i < 10; i++)
            {
                sw.Reset();
                sw.Start();
                items = await itemsClient.GetAllAsync();
                sw.Stop();
                Console.WriteLine($"GetAll: {sw.ElapsedMilliseconds}");
            }

            var item = items.Single(i => i.Id == 1);
            item.Description = "CIAOx!";

            for (var i = 0; i < 10; i++)
            {
                sw.Reset();
                sw.Start();
                await itemsClient.SaveCompleteAsync(item);
                sw.Stop();
                Console.WriteLine($"Save: {sw.ElapsedMilliseconds}");
            }
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
