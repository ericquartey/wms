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
                new MainMenuItem(Navigation.MasterData, bgColor, Icons.MasterData, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.Items, bgColor, nameof(Navigation.Items), nameof(Modules.MasterData), Modules.MasterData.ITEMSANDDETAILS),
                        new MainMenuItem(Navigation.Cells, bgColor, nameof(Navigation.Cells), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.LoadingUnits, bgColor, nameof(Navigation.LoadingUnits), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Compartments, bgColor,  nameof(Navigation.Compartments), nameof(Modules.MasterData), Modules.MasterData.COMPARTMENTSANDDETAILS)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Lists, bgColor, Icons.Lists, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.AllLists, bgColor, nameof(Navigation.AllLists), string.Empty, string.Empty)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Others, bgColor, Icons.Others, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.UnderStockItems, bgColor, nameof(Navigation.UnderStockItems), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.CompartmentsMapping, bgColor, nameof(Navigation.CompartmentsMapping), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Inventory, bgColor, nameof(Navigation.Inventory), string.Empty, string.Empty)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.ImportExport, bgColor, Icons.ImportExport, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.Import, bgColor, nameof(Navigation.Import), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.Export, bgColor, nameof(Navigation.Export), string.Empty, string.Empty)
                    }
                }
            );

            this.menu.Items.Add(
                new MainMenuItem(Navigation.Configuration, bgColor, Icons.Configuration, string.Empty, string.Empty)
                {
                    Children = new List<MainMenuItem>
                    {
                        new MainMenuItem(Navigation.UnitsOfMeasurement, bgColor, nameof(Navigation.UnitsOfMeasurement), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.AbcClasses, bgColor, nameof(Navigation.AbcClasses), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.CellStatuses, bgColor, nameof(Navigation.CellStatuses), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.LoadingUnitStatuses, bgColor, nameof(Navigation.LoadingUnitStatuses), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.CompartmentTypes, bgColor, nameof(Navigation.CompartmentTypes), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.CompartmentStatuses, bgColor, nameof(Navigation.CompartmentStatuses), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.PackageTypes, bgColor, nameof(Navigation.PackageTypes), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.MaterialStatuses, bgColor, nameof(Navigation.MaterialStatuses), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.ListTypes, bgColor,  nameof(Navigation.ListTypes), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.ListStatuses, bgColor, nameof(Navigation.ListStatuses),  string.Empty, string.Empty),
                        new MainMenuItem(Navigation.ListRowStatuses, bgColor, nameof(Navigation.ListRowStatuses), string.Empty, string.Empty),
                        new MainMenuItem(Navigation.MachineTypes, bgColor, nameof(Navigation.MachineTypes), string.Empty, string.Empty)
                    }
                }
            );
        }

        #endregion Methods
    }
}
