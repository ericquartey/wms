using System.Collections.Generic;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Utils.Menu
{
    public class AppMenu
    {
        #region Fields

        private MainMenu menu;

        #endregion Fields

        #region Constructors

        public AppMenu()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public MainMenu Menu => this.menu;

        #endregion Properties

        #region Methods

        private void Initialize()
        {
            this.menu = new MainMenu();
            this.menu.Items = new List<MainMenuItem>();

            const string bgColor = "CommonSecondaryDark";

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Machine_Status, bgColor, Icons.MachineStatus, string.Empty, string.Empty)
                );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Catalog, bgColor, Icons.MasterData, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.Catalog_Items, bgColor, Icons.WarehouseItems, nameof(Modules.Catalog), Modules.Catalog.ITEMSANDDETAILS),
                        new MainMenuItem(Navigation.Catalog_Cells, bgColor, Icons.Cells, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Catalog_LoadingUnits, bgColor, Icons.LoadingUnit, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Catalog_Compartments, bgColor,  Icons.Compartments, nameof(Modules.Catalog), Modules.Catalog.COMPARTMENTSANDDETAILS)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Lists, bgColor, Icons.Lists, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.Lists_Lists, bgColor, Icons.AllLists, string.Empty, string.Empty)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Others, bgColor, Icons.Others, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.Others_UnderStock_Items, bgColor, Icons.UnderStock, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Others_CompartmentsMapping, bgColor, Icons.CompartmentsMapping, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Others_Inventory, bgColor, Icons.Inventory, string.Empty, string.Empty)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.ImportExport, bgColor, Icons.ImportExport, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.ImportExport_Import, bgColor, Icons.Import, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.ImportExport_Export, bgColor, Icons.Export, string.Empty, string.Empty)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Configuration, bgColor, Icons.Configuration, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.Configuration_MeasuringUnits, bgColor, Icons.UnitsOfMeasurement, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_AbcClasses, bgColor, Icons.ItemClasses, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_CellStatuses, bgColor, Icons.CellStatuses, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_LoadingUnitStatuses, bgColor, Icons.LoadingUnitStatuses, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_CompartmentTypes, bgColor, Icons.CompartmentTypes, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_CompartmentStatuses, bgColor, Icons.CompartmentStatuses, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_PackageTypes, bgColor, Icons.PackageTypes, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_MaterialStatuses, bgColor, Icons.MaterialStatuses, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_ListTypes, bgColor,  Icons.ListTypes, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_ListStatuses, bgColor, Icons.ListStatuses,  string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_ListRowStatuses, bgColor, Icons.ListRowStatuses, string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Configuration_MachineTypes, bgColor, Icons.ListRowStatuses, string.Empty, string.Empty)
                    }
                }
            );
        }

        #endregion Methods
    }
}
