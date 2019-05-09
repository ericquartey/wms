using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    public static class Views
    {
        public static int ReadListId()
        {
            Console.Write("Insert list id: ");
            var listIdString = Console.ReadLine();

            if (int.TryParse(listIdString, out var listId))
            {
                return listId;
            }
            else
            {
                Console.WriteLine("Unable to parse list id.");
            }

            return -1;
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
                Console.WriteLine("Insert bay id:");
                Console.Write("> ");

                var bayId = Views.ReadBayId();

                selectedBay = bays.SingleOrDefault(b => b.Id == bayId);
                if (selectedBay == null)
                {
                    Console.WriteLine("Invalid bay selection.");
                }
            }
            while (selectedBay == null);

            return selectedBay;
        }

        public static int ReadMissionId()
        {
            Console.Write("Insert mission id: ");
            var missionIdString = Console.ReadLine();

            if (int.TryParse(missionIdString, out var missionId))
            {
                return missionId;
            }
            else
            {
                Console.WriteLine("Unable to parse mission id.");
            }

            return -1;
        }

        public static int ReadQuantity()
        {
            Console.Write("Insert quantity: ");
            var quantityString = Console.ReadLine();

            if (int.TryParse(quantityString, out var quantity))
            {
                return quantity;
            }
            else
            {
                Console.WriteLine("Unable to parse mission quantity.");
            }

            return -1;
        }

        public static void PrintMachineStatus(VW.AutomationService.Hubs.MachineStatus machineStatus)
        {
            if (machineStatus == null)
            {
                throw new ArgumentNullException(nameof(machineStatus));
            }

            Console.WriteLine($"Mode: {machineStatus.Mode}");

            if (machineStatus.Mode == VW.AutomationService.Hubs.MachineMode.Fault)
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

        public static int ReadBayId()
        {
            Console.Write("Insert bay id: ");
            var bayIdString = Console.ReadLine();

            if (int.TryParse(bayIdString, out var bayId))
            {
                return bayId;
            }
            else
            {
                Console.WriteLine("Unable to parse bay id.");
            }

            return -1;
        }

        public static void PrintMissionsTable(IEnumerable<Mission> missions)
        {
            if (!missions.Any())
            {
                Console.WriteLine("No missions are available.");

                return;
            }

            Console.WriteLine("Available missions (by priority, then by creation date):");

            Console.WriteLine(
                $"| {nameof(Mission.Priority), 8} " +
                $"| {nameof(Mission.Id), 3} " +
                $"| {nameof(Mission.Status), -10} " +
                $"| {nameof(Mission.ItemDescription), -40} " +
                $"| Quantities |");

            Console.WriteLine($"|----------|-----|------------|------------------------------------------|------------|");

            var completedMissions = missions
                   .Where(m => m.Status == MissionStatus.Completed || m.Status == MissionStatus.Incomplete)
                   .OrderBy(m => m.Priority)
                   .ThenBy(m => m.CreationDate);

            foreach (var mission in missions
                                        .Except(completedMissions)
                                        .OrderBy(m => m.Priority)
                                        .ThenBy(m => m.CreationDate))
            {
                PrintMissionTableRow(mission);
            }

            if (completedMissions.Any())
            {
                Console.WriteLine($"|----------|-----|------------|------------------------------------------|------------|");

                foreach (var mission in completedMissions)
                {
                    PrintMissionTableRow(mission);
                }
            }

            Console.WriteLine($"|__________|_____|____________|__________________________________________|____________|");
        }

        public static UserSelection PromptForUserSelection()
        {
            Console.WriteLine();
            Console.WriteLine("Select option: ");
            Console.WriteLine($"{(int)UserSelection.Login} - Login to PPC");
            Console.WriteLine($"{(int)UserSelection.DisplayMachineStatus} - Display machine status");
            Console.WriteLine($"{(int)UserSelection.DisplayMissions} - Display missions");
            Console.WriteLine($"{(int)UserSelection.ExecuteMission} - Execute mission");
            Console.WriteLine($"{(int)UserSelection.CompleteMission} - Complete mission");
            Console.WriteLine($"{(int)UserSelection.DisplayLists} - Display Lists");
            Console.WriteLine($"{(int)UserSelection.ExecuteList} - Execute List");
            Console.WriteLine($"{(int)UserSelection.Exit} - Exit");
            Console.Write("> ");

            var userSelection = UserSelection.NotSpecified;
            do
            {
                var selectionString = Console.ReadLine();

                if (int.TryParse(selectionString, out var selection) == false)
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

        private static void PrintListTableRow(ItemList list)
        {
            Console.WriteLine(
                $"| {list.Priority, 8} | {list.Id, 3} | {list.Status, -10} |");
        }

        private static void PrintMissionTableRow(Mission mission)
        {
            string trimmedDescription = null;
            if (mission.ItemDescription != null)
            {
                trimmedDescription = mission.ItemDescription?.Substring(0, Math.Min(40, mission.ItemDescription.Length));
            }

            var quantities = $"{mission.DispatchedQuantity, 2} / {mission.RequestedQuantity, 2}";

            Console.WriteLine(
                $"| {mission.Priority, 8} | {mission.Id, 3} | {mission.Status, -10} | {trimmedDescription, -40} | {quantities, 10} |");
        }
    }
}
