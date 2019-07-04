using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MachineAutomationService.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public partial class AutomationService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await this.dataHubClient.ConnectAsync();

                var machines = await this.automationProvider.GetMachinesAsync();

                Console.Clear();
                Views.DisplayHeader();

                Machine selectedMachine = null;
                if (machines.Count() == 1)
                {
                    selectedMachine = machines.Single();
                }
                else
                {
                    selectedMachine = Views.PromptForMachineSelection(machines);
                }

                Console.WriteLine($"Current machine: {selectedMachine.Nickname}");

                this.machineStatus.MachineId = selectedMachine.Id;

                var bays = await this.automationProvider.GetBaysAsync(this.machineStatus.MachineId);
                this.machineStatus.BaysStatus = bays.Select(b => new BayStatus { BayId = b.Id });

                this.machineStatus.Mode = MachineMode.Auto;
                await this.machineHub.Clients?.All.ModeChanged(this.machineStatus.Mode, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine();
            }

            var exitRequested = false;

            Views.DisplayUserOptions(this.machineStatus);
            var selection = Views.PromptForUserSelection();
            while (!exitRequested)
            {
                Console.Clear();
                try
                {
                    exitRequested = await this.ExecuteOperationAsync(selection);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    Console.WriteLine();
                }

                Views.DisplayUserOptions(this.machineStatus);
                selection = Views.PromptForUserSelection();
            }

            this.appLifetime.StopApplication();
        }
    }
}
