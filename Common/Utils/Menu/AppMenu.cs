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
            this.menu.Items.Add(new MainMenuItem(
                Navigation.Machine_Status, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/white/dashboard.png",
                string.Empty, string.Empty));

            var catalogMenu = new MainMenuItem(
                Navigation.Catalog, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/white/manual_warehouse.png",
                string.Empty, string.Empty);
            catalogMenu.Children.Add(new MainMenuItem(
                Navigation.Catalog_Items, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/items.png",
                nameof(Modules.Catalog), Modules.Catalog.ITEMSANDDETAILS));
            catalogMenu.Children.Add(new MainMenuItem(
                Navigation.Catalog_Cells, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/cells.png", string.Empty,
                string.Empty));
            catalogMenu.Children.Add(new MainMenuItem(
                Navigation.Catalog_LoadingUnits, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/loading_unit.png",
                string.Empty, string.Empty));
            catalogMenu.Children.Add(new MainMenuItem(
                Navigation.Catalog_Compartments, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_view_compact_grey_24dp.png",
                string.Empty, string.Empty));
            this.menu.Items.Add(catalogMenu);

            var configMenu = new MainMenuItem(
                Navigation.Lists, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/action/ic_list_white_24dp.png",
                string.Empty, string.Empty);
            configMenu.Children.Add(new MainMenuItem(
                Navigation.Lists_Lists, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_list_grey_24dp.png",
                string.Empty, string.Empty));
            this.menu.Items.Add(configMenu);

            var otherMenu = new MainMenuItem(
                Navigation.Others, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/editor/ic_functions_white_24dp.png",
                string.Empty, string.Empty);
            otherMenu.Children.Add(new MainMenuItem(
                Navigation.Others_UnderStock_Items, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_trending_down_grey_24dp.png",
                string.Empty, string.Empty));
            otherMenu.Children.Add(new MainMenuItem(
                Navigation.Others_CompartmentsMapping, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/compartments_mapping.png",
                string.Empty, string.Empty));
            otherMenu.Children.Add(new MainMenuItem(
                Navigation.Others_Inventory, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/inventory.png",
                string.Empty, string.Empty));
            this.menu.Items.Add(otherMenu);

            var importExportMenu = new MainMenuItem(
                Navigation.ImportExport, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/communication/ic_import_export_white_24dp.png",
                string.Empty, string.Empty);
            importExportMenu.Children.Add(new MainMenuItem(
                Navigation.ImportExport_Import, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_arrow_upward_grey_24dp.png",
                string.Empty, string.Empty));
            importExportMenu.Children.Add(new MainMenuItem(
                Navigation.ImportExport_Export, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_arrow_downward_grey_24dp.png",
                string.Empty, string.Empty));
            this.menu.Items.Add(importExportMenu);

            var configurationMenu = new MainMenuItem(
                Navigation.Configuration, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/action/ic_settings_white_24dp.png",
                string.Empty, string.Empty);

            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_MeasuringUnits, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_AbcClasses, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_CellStatuses, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_LoadingUnitStatuses, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_CompartmentTypes, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_CompartmentStatuses, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_PackageTypes, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_MaterialStatuses, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_ListTypes, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_ListStatuses, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_ListRowStatuses, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            configurationMenu.Children.Add(new MainMenuItem(
                Navigation.Configuration_MachineTypes, bgColor,
                "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",
                string.Empty, string.Empty));
            this.menu.Items.Add(configurationMenu);
        }

        #endregion Methods
    }
}
