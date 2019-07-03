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
        #region Fields

        private string machineName;

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine($"Connecting to service ...");
                await this.dataHubClient.ConnectAsync();

                var machines = await this.automationProvider.GetMachinesAsync();

                Console.Clear();
                Views.DisplayHeader(this.machineName);

                Machine selectedMachine = null;
                if (machines.Count() == 1)
                {
                    selectedMachine = machines.Single();
                }
                else
                {
                    selectedMachine = Views.PromptForMachineSelection(machines);
                }

                Console.Clear();

                this.machineName = selectedMachine.Nickname;
                Views.DisplayUserOptions(this.machineStatus, this.machineName);

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
            var selection = Views.PromptForUserSelection();

            while (!exitRequested)
            {
                try
                {
                    Console.Clear();
                    exitRequested = await this.ExecuteOperationAsync(selection);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    Console.WriteLine();
                }

                if (!exitRequested)
                {
                    Views.DisplayUserOptions(this.machineStatus, this.machineName);
                    selection = Views.PromptForUserSelection();
                }
            }

            Console.WriteLine($"Shutting down Panel PC ...");
            this.appLifetime.StopApplication();
        }

        #endregion
    }
}
