using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Modules.Layout
{
    public class AppMenu
    {
        #region Fields

        private MainMenu menu;

        #endregion

        #region Constructors

        public AppMenu()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public MainMenu Menu => this.menu;

        #endregion

        #region Methods

        private void Initialize()
        {
            this.menu = new MainMenu();

            const string bgColor = "CommonSecondaryDark";

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Machines, bgColor, Icons.MachineStatus, nameof(Common.Utils.Modules.Machines), Common.Utils.Modules.Machines.MACHINES));

            var masterDataMenu =
                new MainMenuItem(Navigation.MasterData, bgColor, Icons.MasterData, string.Empty, string.Empty);
            this.menu.Items.Add(masterDataMenu);
            masterDataMenu.Children.AddRange(new[]
            {
                new MainMenuItem(
                    Navigation.Items,
                    bgColor,
                    nameof(Navigation.Items),
                    nameof(Common.Utils.Modules.MasterData),
                    Common.Utils.Modules.MasterData.ITEMS),
                new MainMenuItem(
                    Navigation.Cells,
                    bgColor,
                    nameof(Navigation.Cells),
                    nameof(Common.Utils.Modules.MasterData),
                    Common.Utils.Modules.MasterData.CELLS),
                new MainMenuItem(
                    Navigation.LoadingUnits,
                    bgColor,
                    nameof(Navigation.LoadingUnits),
                    nameof(Common.Utils.Modules.MasterData),
                    Common.Utils.Modules.MasterData.LOADINGUNITS),
                new MainMenuItem(
                    Navigation.Compartments,
                    bgColor,
                    nameof(Navigation.Compartments),
                    nameof(Common.Utils.Modules.MasterData),
                    Common.Utils.Modules.MasterData.COMPARTMENTS),
                new MainMenuItem(
                    Navigation.CompartmentTypes,
                    bgColor,
                    nameof(Navigation.CompartmentTypes),
                    nameof(Common.Utils.Modules.MasterData),
                    Common.Utils.Modules.MasterData.COMPARTMENTTYPES),
            });

            var listsMenu = new MainMenuItem(Navigation.Lists, bgColor, Icons.Lists, string.Empty, string.Empty);
            this.menu.Items.Add(listsMenu);
            listsMenu.Children.AddRange(new[]
            {
                new MainMenuItem(
                    Navigation.AllLists,
                    bgColor,
                    nameof(Navigation.AllLists),
                    nameof(Common.Utils.Modules.ItemLists),
                    Common.Utils.Modules.ItemLists.ITEMLISTS),
                new MainMenuItem(
                    Navigation.PickLists,
                    bgColor,
                    nameof(Navigation.PickLists),
                    nameof(Common.Utils.Modules.ItemLists),
                    Common.Utils.Modules.ItemLists.ITEMLISTS,
                    Common.Utils.Modules.ItemLists.ITEMLISTSPICK),
                new MainMenuItem(
                    Navigation.PutLists,
                    bgColor,
                    nameof(Navigation.PutLists),
                    nameof(Common.Utils.Modules.ItemLists),
                    Common.Utils.Modules.ItemLists.ITEMLISTS,
                    Common.Utils.Modules.ItemLists.ITEMLISTSPUT),
                new MainMenuItem(
                    Navigation.InventoryLists,
                    bgColor,
                    nameof(Navigation.InventoryLists),
                    nameof(Common.Utils.Modules.ItemLists),
                    Common.Utils.Modules.ItemLists.ITEMLISTS,
                    Common.Utils.Modules.ItemLists.ITEMLISTSINVENTORY),
            });

            var schedulerMenu =
                new MainMenuItem(Navigation.Scheduler, bgColor, Icons.Scheduler, string.Empty, string.Empty);
            this.menu.Items.Add(schedulerMenu);
            schedulerMenu.Children.AddRange(new[]
            {
                new MainMenuItem(
                    Navigation.SchedulerRequests,
                    bgColor,
                    nameof(Navigation.SchedulerRequests),
                    nameof(Common.Utils.Modules.Scheduler),
                    Common.Utils.Modules.Scheduler.SCHEDULERREQUESTS),
                new MainMenuItem(
                    Navigation.Missions,
                    bgColor,
                    nameof(Navigation.Missions),
                    nameof(Common.Utils.Modules.Scheduler),
                    Common.Utils.Modules.Scheduler.MISSIONS),
            });

            var importExportMenu = new MainMenuItem(
                Navigation.ImportExport,
                bgColor,
                Icons.ImportExport,
                string.Empty,
                string.Empty);
            this.menu.Items.Add(importExportMenu);
            importExportMenu.Children.AddRange(new[]
            {
                   new MainMenuItem(Navigation.Import, bgColor, nameof(Navigation.Import), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.Export, bgColor, nameof(Navigation.Export), string.Empty, string.Empty),
            });

            var configurationMenu = new MainMenuItem(
                Navigation.Configuration,
                bgColor,
                Icons.Configuration,
                string.Empty,
                string.Empty);
            this.menu.Items.Add(configurationMenu);

            var othersMenu = new MainMenuItem(Navigation.Others, bgColor, Icons.Others, string.Empty, string.Empty);
            this.menu.Items.Add(othersMenu);
            othersMenu.Children.AddRange(new[]
            {
                   new MainMenuItem(Navigation.UnderStockItems, bgColor, nameof(Navigation.UnderStockItems), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.Inventory, bgColor, nameof(Navigation.Inventory), string.Empty, string.Empty),
            });
        }

        #endregion
    }
}
