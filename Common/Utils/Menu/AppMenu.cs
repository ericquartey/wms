﻿using Ferretto.Common.Resources;

namespace Ferretto.Common.Utils.Menu
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
                new MainMenuItem(Navigation.Machines, bgColor, Icons.MachineStatus, nameof(Modules.Machines), Modules.Machines.MACHINES));

            var masterDataMenu =
                new MainMenuItem(Navigation.MasterData, bgColor, Icons.MasterData, string.Empty, string.Empty);
            this.menu.Items.Add(masterDataMenu);
            masterDataMenu.Children.AddRange(new[]
            {
                new MainMenuItem(Navigation.Items, bgColor, nameof(Navigation.Items), nameof(Modules.MasterData), Modules.MasterData.ITEMS),
                new MainMenuItem(Navigation.Cells, bgColor, nameof(Navigation.Cells),  nameof(Modules.MasterData), Modules.MasterData.CELLS),
                new MainMenuItem(Navigation.LoadingUnits, bgColor, nameof(Navigation.LoadingUnits), nameof(Modules.MasterData), Modules.MasterData.LOADINGUNITS),
                new MainMenuItem(Navigation.Compartments, bgColor,  nameof(Navigation.Compartments), nameof(Modules.MasterData), Modules.MasterData.COMPARTMENTS)
            });

            var listsMenu = new MainMenuItem(Navigation.Lists, bgColor, Icons.Lists, string.Empty, string.Empty);
            this.menu.Items.Add(listsMenu);
            listsMenu.Children.AddRange(new[]
            {
                new MainMenuItem(
                    Navigation.AllLists,
                    bgColor,
                    nameof(Navigation.AllLists),
                    nameof(Modules.ItemLists),
                    Modules.ItemLists.ITEMLISTS),
                new MainMenuItem(
                    Navigation.PickLists,
                    bgColor,
                    nameof(Navigation.PickLists),
                    nameof(Modules.ItemLists),
                    Modules.ItemLists.ITEMLISTS,
                    Modules.ItemLists.ITEMLISTSPICK),
                new MainMenuItem(
                    Navigation.PutLists,
                    bgColor,
                    nameof(Navigation.PutLists),
                    nameof(Modules.ItemLists),
                    Modules.ItemLists.ITEMLISTS,
                    Modules.ItemLists.ITEMLISTSPUT),
                new MainMenuItem(
                    Navigation.InventoryLists,
                    bgColor,
                    nameof(Navigation.InventoryLists),
                    nameof(Modules.ItemLists),
                    Modules.ItemLists.ITEMLISTS,
                    Modules.ItemLists.ITEMLISTSINVENTORY)
            });

            var schedulerMenu =
                new MainMenuItem(Navigation.Scheduler, bgColor, Icons.Scheduler, string.Empty, string.Empty);
            this.menu.Items.Add(schedulerMenu);
            schedulerMenu.Children.AddRange(new[]
            {
                   new MainMenuItem(Navigation.Missions, bgColor, nameof(Navigation.Missions), nameof(Modules.Scheduler), Modules.Scheduler.MISSIONS),
                   new MainMenuItem(Navigation.SchedulerRequests, bgColor, nameof(Navigation.SchedulerRequests), nameof(Modules.Scheduler), Modules.Scheduler.SCHEDULERREQUESTS),
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
                   new MainMenuItem(Navigation.Export, bgColor, nameof(Navigation.Export), string.Empty, string.Empty)
               });

            var configurationMenu = new MainMenuItem(
                Navigation.Configuration,
                bgColor,
                Icons.Configuration,
                string.Empty,
                string.Empty);
            this.menu.Items.Add(configurationMenu);
            configurationMenu.Children.AddRange(new[]
            {
                   new MainMenuItem(Navigation.MeasureUnits, bgColor, nameof(Navigation.MeasureUnits), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.AbcClasses, bgColor, nameof(Navigation.AbcClasses), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.CellStatuses, bgColor, nameof(Navigation.CellStatuses), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.LoadingUnitStatuses, bgColor, nameof(Navigation.LoadingUnitStatuses), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.CompartmentTypes, bgColor, nameof(Navigation.CompartmentTypes), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.CompartmentStatuses, bgColor, nameof(Navigation.CompartmentStatuses), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.PackageTypes, bgColor, nameof(Navigation.PackageTypes), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.MaterialStatuses, bgColor, nameof(Navigation.MaterialStatuses), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.MachineTypes, bgColor, nameof(Navigation.MachineTypes), string.Empty, string.Empty)
               });

            var othersMenu = new MainMenuItem(Navigation.Others, bgColor, Icons.Others, string.Empty, string.Empty);
            this.menu.Items.Add(othersMenu);
            othersMenu.Children.AddRange(new[]
            {
                   new MainMenuItem(Navigation.UnderStockItems, bgColor, nameof(Navigation.UnderStockItems), string.Empty, string.Empty),
                   new MainMenuItem(Navigation.Inventory, bgColor, nameof(Navigation.Inventory), string.Empty, string.Empty)
               });
        }

        #endregion
    }
}
