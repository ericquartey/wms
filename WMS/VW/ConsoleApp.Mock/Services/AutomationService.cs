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

        private Bay activeBay;

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

        public async Task CompleteOperationAsync(int operationId, int quantity)
        {
            try
            {
                var operation = await this.automationProvider.CompleteOperationAsync(operationId, quantity);
                var mission = await this.automationProvider.GetMissionByIdAsync(operation.MissionId);
                if (mission.BayId.HasValue)
                {
                    Console.Write("Moving tray from bay to elevator ... ");

                    await Task.Delay(2000);

                    this.machineStatus
                        .BaysStatus
                        .Single(b => b.BayId == mission.BayId.Value)
                        .LoadingUnitId = null;
                    await this.machineHub.Clients?.All.LoadingUnitInBayChanged(mission.BayId.Value, null);

                    this.machineStatus.ElevatorStatus.LoadingUnitId = mission.LoadingUnitId;
                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(mission.LoadingUnitId);

                    Console.WriteLine("done.");

                    await this.MoveElevatorAsync(0, 100);

                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(null);
                    this.machineStatus.ElevatorStatus.LoadingUnitId = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to complete operation with id={operationId}: {ex.Message}");
                throw;
            }
        }

        public async Task ExecuteListAsync(int listId)
        {
            var bay = await this.GetBayAsync();

            await this.automationProvider.ExecuteListAsync(listId, bay.AreaId, bay.Id);
        }

        public async Task ExecuteOperationAsync(int operationId)
        {
            try
            {
                var operation = await this.automationProvider.ExecuteOperationAsync(operationId);
                var mission = await this.automationProvider.GetMissionByIdAsync(operation.MissionId);

                if (mission.BayId.HasValue)
                {
                    this.machineStatus.ElevatorStatus.LoadingUnitId = mission.LoadingUnitId;
                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(mission.LoadingUnitId);

                    await this.MoveElevatorAsync(100, 0);
                    Console.Write("Moving tray into bay ... ");

                    await Task.Delay(2000);
                    this.machineStatus.ElevatorStatus.LoadingUnitId = null;
                    await this.machineHub.Clients?.All.LoadingUnitInElevatorChanged(null);

                    this.machineStatus.BaysStatus
                       .Single(b => b.BayId == mission.BayId.Value)
                       .LoadingUnitId = mission.LoadingUnitId;
                    await this.machineHub.Clients?.All.LoadingUnitInBayChanged(mission.BayId.Value, mission.LoadingUnitId);

                    Console.WriteLine("done.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to execute operation with id={operationId}: {ex.Message}");
                throw;
            }
        }

        public async Task<Bay> GetBayAsync() => await this.automationProvider.GetBayAsync(
                                    this.configuration.GetValue<int>("Warehouse:Bay:Id"));

        public async Task NotifyUserLoginAsync()
        {
            var bays = await this.automationProvider.GetBaysAsync(this.machineStatus.MachineId);

            this.activeBay = null;
            if (bays.Count() == 1)
            {
                this.activeBay = bays.Single();
            }
            else
            {
                this.activeBay = Views.PromptForBaySelection(bays);
            }

            Console.WriteLine($"Logging to bay: {this.activeBay.Description}");

            await this.machineHub.Clients?.All.UserChanged(1, this.activeBay.Id);
        }

        private static bool ElevatorReachedTargetPosition(decimal position, decimal startPosition, decimal targetPosition)
        {
            return startPosition > targetPosition ? position <= targetPosition : position >= targetPosition;
        }

        private async Task AbortMissionOperationAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var abortOperationId = Views.ReadInt("Insert mission operation id:");
            if (abortOperationId >= 0)
            {
                await this.automationProvider.AbortOperationAsync(abortOperationId);
                Console.WriteLine($"Mission execution request sent.");
            }
        }

        private async Task CompleteMissionActionAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var missionId = Views.ReadInt("Insert mission id:");
            var quantity = Views.ReadInt("Insert mission quantity:");
            if (missionId >= 0)
            {
                if (quantity > 0)
                {
                    await this.CompleteOperationAsync(missionId, quantity);
                }
                else
                {
                    await this.automationProvider.CompleteLoadingUnitMissionAsync(missionId);
                }

                Console.WriteLine($"Request sent.");
            }
        }

        private async Task CompleteLoadingUnitMissionActionAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var missionId = Views.ReadInt("Insert mission id:");
            if (missionId >= 0)
            {
                await this.automationProvider.CompleteLoadingUnitMissionAsync(missionId);
                Console.WriteLine($"Request sent.");
            }
        }

        private async Task ExecuteLoadingUnitMissionActionAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var missionId = Views.ReadInt("Insert mission id:");
            if (missionId >= 0)
            {
                await this.automationProvider.ExecuteLoadingUnitMissionAsync(missionId);
                Console.WriteLine($"Request sent.");
            }
        }

        private async Task ExecuteOperationActionAsync()
        {
            var missions = await this.automationProvider.GetMissionsAsync(this.machineStatus.MachineId);
            Views.PrintMissionsTable(missions);

            var operationId = Views.ReadInt("Insert mission operation id:");
            if (operationId >= 0)
            {
                await this.ExecuteOperationAsync(operationId);
                Console.WriteLine($"Operation execution request sent.");
            }
        }

        private async Task<bool> ExecuteOperationAsync(UserSelection selection)
        {
            var exitRequested = false;
            switch (selection)
            {
                case UserSelection.CompleteOperation:
                    await this.CompleteMissionActionAsync();

                    break;

                case UserSelection.ExecuteOperation:
                    await this.ExecuteOperationActionAsync();

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

                        var faultCode = new Random().Next(10000);

                        await this.machineHub.Clients?.All.ModeChanged(this.machineStatus.Mode, faultCode);
                        break;
                    }

                case UserSelection.AbortOperation:
                    {
                        await this.AbortMissionOperationAsync();

                        break;
                    }

                case UserSelection.ExecuteList:
                    var executeListId = Views.ReadInt("Insert list id:");
                    if (executeListId >= 0)
                    {
                        await this.ExecuteListAsync(executeListId);
                        Console.WriteLine($"List execution request sent.");
                    }

                    break;

                case UserSelection.DisplayOperation:

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
                    await this.LogOutUserAsync();
                    exitRequested = true;
                    break;

                case UserSelection.Login:
                    // notify the scheduler that a user logged in
                    // to the PanelPC associated to the specified bay
                    await this.NotifyUserLoginAsync();
                    Console.WriteLine($"Request sent.");
                    break;

                case UserSelection.CompleteLoadingUnitMission:
                    await this.CompleteLoadingUnitMissionActionAsync();

                    break;
                case UserSelection.ExecuteLoadingUnitMission:
                    await this.ExecuteLoadingUnitMissionActionAsync();
                    break;

                default:
                    Console.WriteLine("Invalid selection.");
                    break;
            }

            return exitRequested;
        }

        private async Task LogOutUserAsync()
        {
            if (this.activeBay != null)
            {
                await this.machineHub.Clients?.All.UserChanged(null, this.activeBay.Id);
                this.activeBay = null;
            }
        }

        private async Task MoveElevatorAsync(decimal startPosition, decimal targetPosition)
        {
            Console.Write("Moving elevator ");

            var increment = (targetPosition - startPosition) / 50;
            var position = startPosition;
            while (!ElevatorReachedTargetPosition(position, startPosition, targetPosition))
            {
                this.machineStatus.ElevatorStatus.Position = position;

                await Task.Delay(70);
                Console.Write($".");

                position += increment;
                await this.machineHub.Clients?.All.ElevatorPositionChanged(position);
            }

            Console.WriteLine(" done.");
        }

        #endregion
    }
}
