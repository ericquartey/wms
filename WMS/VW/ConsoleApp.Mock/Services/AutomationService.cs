using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MachineAutomationService.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public partial class AutomationService : BackgroundService
    {
        #region Fields

        private readonly IApplicationLifetime appLifetime;

        private readonly IAutomationProvider automationProvider;

        private readonly IConfiguration configuration;

        private readonly IDataHubClient dataHubClient;

        private readonly IHubContext<MachineHub, IMachineHub> machineHub;

        private readonly VW.MachineAutomationService.Hubs.MachineStatus machineStatus;

        #endregion

        #region Constructors

        public AutomationService(
            IDataHubClient dataHubClient,
            IHubContext<MachineHub, IMachineHub> machineHub,
            ILiveMachineDataContext liveMachineDataContext,
            IConfiguration configuration,
            IAutomationProvider automationProvider,
            IItemListsDataService listsDataService,
            IApplicationLifetime appLifetime)
        {
            if (liveMachineDataContext == null)
            {
                throw new ArgumentNullException(nameof(liveMachineDataContext));
            }

            this.configuration = configuration;
            this.automationProvider = automationProvider;

            this.dataHubClient = dataHubClient;
            this.machineHub = machineHub;
            this.machineStatus = liveMachineDataContext.MachineStatus;
            this.appLifetime = appLifetime;
        }

        #endregion

        #region Methods

        public async Task CompleteMissionAsync(int missionId, int quantity)
        {
            try
            {
                var mission = await this.automationProvider.CompleteMissionAsync(missionId, quantity);

                if (mission.BayId.HasValue)
                {
                    var loadingUnitId = await this.automationProvider.GetLoadingUnitIdFromMissionAsync(mission);

                    Console.Write("Moving tray from bay to elevator ... ");

                    await Task.Delay(1000);

                    this.machineStatus
                        .BaysStatus
                        .Single(b => b.BayId == mission.BayId.Value)
                        .LoadingUnitId = null;
                    await this.machineHub.Clients?.All.LoadingUnitInBayChanged(mission.BayId.Value, null);

                    this.machineStatus.ElevatorStatus.LoadingUnitId = loadingUnitId;
                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(loadingUnitId);

                    Console.WriteLine("done.");

                    await this.MoveElevatorAsync(0, 100);

                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(null);
                    this.machineStatus.ElevatorStatus.LoadingUnitId = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to complete mission with id={missionId}: {ex.Message}");
                throw;
            }
        }

        public async Task ExecuteListAsync(int listId)
        {
            var bay = await this.GetBayAsync();

            await this.automationProvider.ExecuteListAsync(listId, bay.AreaId, bay.Id);
        }

        public async Task ExecuteMissionAsync(int missionId)
        {
            try
            {
                var mission = await this.automationProvider.ExecuteMissionAsync(missionId);

                if (mission.BayId.HasValue)
                {
                    var loadingUnitId = await this.automationProvider.GetLoadingUnitIdFromMissionAsync(mission);

                    this.machineStatus.ElevatorStatus.LoadingUnitId = loadingUnitId;
                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(loadingUnitId);

                    await this.MoveElevatorAsync(100, 0);
                    Console.Write("Moving tray into bay ... ");

                    await Task.Delay(1000);
                    this.machineStatus.ElevatorStatus.LoadingUnitId = null;
                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(null);

                    this.machineStatus.BaysStatus
                       .Single(b => b.BayId == mission.BayId.Value)
                       .LoadingUnitId = loadingUnitId;
                    await this.machineHub.Clients?.All.LoadingUnitInBayChanged(mission.BayId.Value, loadingUnitId);

                    Console.WriteLine("done.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to execute mission with id={missionId}: {ex.Message}");
                throw;
            }
        }

        public async Task<Bay> GetBayAsync() => await this.automationProvider.GetBayAsync(
                                    this.configuration.GetValue<int>("Warehouse:Bay:Id"));

        public async Task NotifyUserLoginAsync()
        {
            var bays = await this.automationProvider.GetBaysAsync(this.machineStatus.MachineId);

            Bay selectedBay = null;
            if (bays.Count() == 1)
            {
                selectedBay = bays.Single();
            }
            else
            {
                selectedBay = Views.PromptForBaySelection(bays);
            }

            Console.WriteLine($"Logging to bay: {selectedBay.Description}");

            await this.automationProvider.ActivateBayAsync(selectedBay.Id);
        }

        private static bool ElevatorReachedTargetPosition(decimal position, decimal startPosition, decimal targetPosition)
        {
            return startPosition > targetPosition ? position <= targetPosition : position >= targetPosition;
        }

        private async Task<bool> ExecuteOperationAsync(UserSelection selection)
        {
            var exitRequested = false;
            switch (selection)
            {
                case UserSelection.CompleteMission:
                    await this.CompleteMissionActionAsync();

                    break;

                case UserSelection.ExecuteMission:
                    await this.ExecuteMissionActionAsync();

                    break;

                case UserSelection.ToggleMachineMode:
                    {
                        var newMode = this.machineStatus.Mode == MachineMode.Auto ? MachineMode.Manual : MachineMode.Auto;

                        this.machineStatus.Mode = newMode;

                        await this.machineHub.Clients?.All.ModeChanged(newMode, null);
                        Console.WriteLine($"Machine mode switched to '{newMode}'.");
                        break;
                    }

                case UserSelection.SetMachineFault:
                    {
                        this.machineStatus.Mode = MachineMode.Fault;

                        const int faultCode = 0;

                        await this.machineHub.Clients?.All.ModeChanged(this.machineStatus.Mode, faultCode);
                        break;
                    }

                case UserSelection.AbortMission:
                    {
                        await this.AbortMissionOperationAsync();

                        break;
                    }

                case UserSelection.ExecuteList:
                    var executeListId = Views.ReadListId();
                    if (executeListId >= 0)
                    {
                        await this.ExecuteListAsync(executeListId);
                        Console.WriteLine($"List execution request sent.");
                    }

                    break;

                case UserSelection.DisplayMissions:

                    var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);

                    Views.PrintMissionsTable(missions);

                    break;

                case UserSelection.DisplayLists:

                    var lists = await this.automationProvider.GetListsAsync();

                    Views.PrintListsTable(lists);

                    break;

                case UserSelection.DisplayMachineStatus:
                    Views.PrintMachineStatus(this.machineStatus);
                    break;

                case UserSelection.Exit:
                    exitRequested = true;
                    break;

                case UserSelection.Login:
                    // notify the scheduler that a user logged in
                    // to the PanelPC associated to the specified bay
                    await this.NotifyUserLoginAsync();
                    Console.WriteLine($"Request sent.");
                    break;

                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }

            return exitRequested;
        }

        private async Task AbortMissionOperationAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var abortMissionId = Views.ReadMissionId();
            if (abortMissionId >= 0)
            {
                await this.automationProvider.AbortMissionAsync(abortMissionId);
                Console.WriteLine($"Mission execution request sent.");
            }
        }

        private async Task ExecuteMissionActionAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var executeMissionId = Views.ReadMissionId();
            if (executeMissionId >= 0)
            {
                await this.ExecuteMissionAsync(executeMissionId);
                Console.WriteLine($"Mission execution request sent.");
            }
        }

        private async Task CompleteMissionActionAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var missionId = Views.ReadMissionId();
            var quantity = Views.ReadQuantity();
            if (missionId >= 0)
            {
                if (quantity > 0)
                {
                    await this.CompleteMissionAsync(missionId, quantity);
                }
                else
                {
                    await this.automationProvider.CompleteLoadingUnitMissionAsync(missionId);
                }

                Console.WriteLine($"Request sent.");
            }
        }

        private async Task MoveElevatorAsync(decimal startPosition, decimal targetPosition)
        {
            Console.Write("Moving elevator ");

            var increment = (targetPosition - startPosition) / 50;
            var position = startPosition;
            while (ElevatorReachedTargetPosition(position, startPosition, targetPosition) == false)
            {
                this.machineStatus.ElevatorStatus.Position = position;
                await this.machineHub.Clients?.All.ElevatorPositionChanged(position);

                await Task.Delay(50);
                Console.Write($".");

                position += increment;
            }

            Console.WriteLine(" done.");
        }

        #endregion
    }
}
