using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public static class Views
    {
        #region Methods

        public static void DisplayHeader(string machineName)
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("   VertiMAG Panel PC   ");

            if (!string.IsNullOrEmpty(machineName))
            {
                Console.WriteLine($"   {machineName}");
            }

            Console.WriteLine("-----------------------");
        }

        public static void DisplayUserOptions(VW.MachineAutomationService.Hubs.MachineStatus machineStatus, string machineName)
        {
            if (machineStatus == null)
            {
                throw new ArgumentNullException(nameof(machineStatus));
            }

            DisplayHeader(machineName);

            Console.WriteLine($"{(int)UserSelection.Login} - Login to PPC");
            Console.WriteLine($"{(int)UserSelection.Exit} - Exit");
            Console.WriteLine();
            Console.WriteLine("Machine");
            Console.WriteLine("-------");
            Console.WriteLine($"{(int)UserSelection.DisplayMachineStatus} - Display Status");
            Console.WriteLine($"{(int)UserSelection.ToggleMachineMode} - Auto/Manual Toggle (current: {machineStatus.Mode})");
            Console.WriteLine($"{(int)UserSelection.SetMachineFault} - Set Fault");
            Console.WriteLine();
            Console.WriteLine("Missions");
            Console.WriteLine("--------");
            Console.WriteLine($"{(int)UserSelection.DisplayOperation} - Display missions");
            Console.WriteLine($"{(int)UserSelection.ExecuteOperation} - Execute mission");
            Console.WriteLine($"{(int)UserSelection.CompleteOperation} - Complete mission");
            Console.WriteLine($"{(int)UserSelection.AbortOperation} - Abort mission");
            Console.WriteLine();
            Console.WriteLine("Lists");
            Console.WriteLine("-----");
            Console.WriteLine($"{(int)UserSelection.DisplayLists} - Display Lists");
            Console.WriteLine($"{(int)UserSelection.ExecuteList} - Execute List");
            Console.WriteLine("Loading Unit");
            Console.WriteLine("-----");
            Console.WriteLine($"{(int)UserSelection.ExecuteLoadingUnitMission} - Execute LU Mission");
            Console.WriteLine($"{(int)UserSelection.CompleteLoadingUnitMission} - Complete LU Mission");
        }

        public static void PrintListsTable(IEnumerable<ItemList> lists)
        {
            if (lists == null)
            {
                return;
            }

            if (!lists.Any())
            {
                Console.WriteLine("No lists are available.");

                return;
            }

            Console.WriteLine("Lists (by priority):");

            Console.WriteLine(
                $"| {nameof(ItemList.Priority), 8} " +
                $"| {nameof(ItemList.Id), 3} " +
                $"| {nameof(ItemList.Status), -10} " +
                $"| Quantities |");

            Console.WriteLine($"|----------|-----|------------|");

            foreach (var list in lists)
            {
                PrintListTableRow(list);
            }

            Console.WriteLine($"|__________|_____|____________|");
        }

        public static void PrintMachineStatus(VW.MachineAutomationService.Hubs.MachineStatus machineStatus)
        {
            if (machineStatus == null)
            {
                throw new ArgumentNullException(nameof(machineStatus));
            }

            Console.WriteLine($"Mode: {machineStatus.Mode}");

            if (machineStatus.Mode == VW.MachineAutomationService.Hubs.MachineMode.Fault)
            {
                Console.WriteLine($"Fault Code: {machineStatus.FaultCode}");
            }

            Console.WriteLine($"Elevator position: {machineStatus.ElevatorStatus.Position}");
            Console.WriteLine($"Elevator tray: {machineStatus.ElevatorStatus.LoadingUnitId}");

            foreach (var bayStatus in machineStatus.BaysStatus)
            {
                Console.WriteLine($"Bay id#{bayStatus.BayId}");
                Console.WriteLine($"Bay tray: {bayStatus.LoadingUnitId}");
                Console.WriteLine($"Bay user: {bayStatus.LoggedUserId}");
            }
        }

        public static void PrintMissionsTable(IEnumerable<MissionInfo> missions)
        {
            const string separatorLine =
                "|----|----------|-----|------------|------------------------------------------|------------|";

            if (missions == null)
            {
                throw new ArgumentNullException(nameof(missions));
            }

            if (!missions.Any())
            {
                Console.WriteLine("No missions are available.");

                return;
            }

            Console.WriteLine("Available missions (by priority):");

            Console.WriteLine(
                "|    " +
                $"| {nameof(Mission.Priority), 8} " +
                $"| {nameof(Mission.Id), 3} " +
                $"| {nameof(Mission.Status), -10} " +
                $"| {nameof(MissionOperationInfo.ItemDescription), -40} " +
                "| Quantities |");

            Console.WriteLine(separatorLine);

            var completedMissions = missions
                   .Where(m => m.Status == MissionStatus.Completed || m.Status == MissionStatus.Incomplete)
                   .OrderBy(m => m.Priority);

            foreach (var mission in missions
                .Except(completedMissions)
                .OrderBy(m => m.Priority))
            {
                PrintMissionTableRow(mission);
            }

            if (completedMissions.Any())
            {
                Console.WriteLine(separatorLine);

                foreach (var mission in completedMissions)
                {
                    PrintMissionTableRow(mission);
                }
            }

            Console.WriteLine("|____|__________|_____|____________|__________________________________________|____________|");
        }

        public static Bay PromptForBaySelection(IEnumerable<Bay> bays)
        {
            if (bays == null)
            {
                throw new ArgumentNullException(nameof(bays));
            }

            Bay selectedBay;

            Console.WriteLine("On which bay are you logging in?");
            foreach (var bay in bays)
            {
                Console.WriteLine($"{bay.Description} (id: {bay.Id})");
            }

            do
            {
                var bayId = Views.ReadInt("Insert bay id:");

                selectedBay = bays.SingleOrDefault(b => b.Id == bayId);
                if (selectedBay == null)
                {
                    Console.WriteLine("Invalid bay selection.");
                }
            }
            while (selectedBay == null);

            return selectedBay;
        }

        public static Machine PromptForMachineSelection(IEnumerable<Machine> machines)
        {
            Machine selectedMachine = null;

            Console.WriteLine($"Machine: 'Who am I?'");
            foreach (var machine in machines)
            {
                Console.WriteLine($"{machine.Nickname} (id: {machine.Id})");
            }

            do
            {
                Console.WriteLine("Insert machine id:");
                Console.Write("> ");

                var machineIdString = Console.ReadLine();
                if (int.TryParse(machineIdString, out var machineId))
                {
                    selectedMachine = machines.SingleOrDefault(m => m.Id == machineId);
                }
                else
                {
                    Console.WriteLine("Invalid machine id selection.");
                }
            }
            while (selectedMachine == null);

            return selectedMachine;
        }

        public static UserSelection PromptForUserSelection()
        {
            Console.WriteLine("Select option:");
            Console.Write("> ");

            var userSelection = UserSelection.NotSpecified;
            do
            {
                var selectionString = Console.ReadLine();

                if (!int.TryParse(selectionString, out var selection))
                {
                    Console.WriteLine("Unable to parse selection.");
                    Console.Write("> ");
                }
                else
                {
                    userSelection = (UserSelection)selection;
                }
            }
            while (userSelection == UserSelection.NotSpecified);

            return userSelection;
        }

        public static int ReadInt(string promptMessage)
        {
            Console.WriteLine(promptMessage);
            Console.Write("> ");
            var quantityString = Console.ReadLine();

            if (int.TryParse(quantityString, out var quantity))
            {
                return quantity;
            }
            else
            {
                Console.WriteLine("Unable to parse value.");
            }

            return -1;
        }

        private static void PrintListTableRow(ItemList list)
        {
            Console.WriteLine(
                $"| {list.Priority, 8} | {list.Id, 3} | {list.Status, -10} |");
        }

        private static void PrintMissionTableRow(MissionInfo mission)
        {
            if (mission.Operations.Count > 0)
            {
                Console.WriteLine(
                    $"| +  | {mission.Priority, 8} |     | {mission.Status, -10} |                                          |            |");

                foreach (var operation in mission.Operations)
                {
                    var trimmedDescription = operation.ItemDescription.Substring(0, Math.Min(40, operation.ItemDescription.Length));
                    var quantities = $"{operation.DispatchedQuantity, 2} / {operation.RequestedQuantity, 2}";

                    Console.WriteLine(
                   $"|  > | {operation.Priority, 8} | {operation.Id, 3} | {operation.Status, -10} | {trimmedDescription, -40} | {quantities, 10} |");
                }
            }
            else
            {
                Console.WriteLine(
                   $"| >> | {mission.Priority, 8} | {mission.Id, 3} | {mission.Status, -10} | Loading Unit                             |            |");
            }
        }

        #endregion
    }
}
