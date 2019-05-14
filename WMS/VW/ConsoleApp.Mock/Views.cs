using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.PanelPC.ConsoleApp.Mock
{
    internal static class Views
    {
        #region Methods

        public static string GetConsoleSecurePassword()
        {
            var pwd = new StringBuilder();
            while (true)
            {
                var i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    return pwd.ToString();
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    pwd.Remove(pwd.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else
                {
                    pwd.Append(i.KeyChar);
                    Console.Write("*");
                }
            }
        }

        public static int GetListId()
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

        public static int GetMissionId()
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

        public static int GetQuantity()
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

        public static void PrintListsTable(IEnumerable<ItemList> lists)
        {
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

        public static void PrintListTableRow(ItemList list)
        {
            Console.WriteLine(
                $"| {list.Priority, 8} | {list.Id, 3} | {list.Status, -10} |");
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

        public static void PrintMissionTableRow(Mission mission)
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

        #endregion
    }
}
