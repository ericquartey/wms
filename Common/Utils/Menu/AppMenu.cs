using System.Collections.Generic;

namespace Ferretto.Common.Utils.Menu
{
  public class AppMenu
  {
    private MainMenu menu;
    public MainMenu Menu => this.menu;

    public AppMenu()
    {
      this.Initialize();
    }

    private void Initialize()
    {
      var bgColor = "#525252";
      this.menu = new MainMenu();
      this.menu.Items = new List<MainMenuItem>();

      this.menu.Items.Add(new MainMenuItem(Resources.Resources.Nav_Machine_Status, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/white/dashboard.png",  string.Empty, string.Empty));

      var catalogMenu = new MainMenuItem(Resources.Resources.Nav_Catalog, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/white/manual_warehouse.png",  string.Empty, string.Empty);
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_Items, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/items.png",  nameof(Modules.Catalog), Modules.Catalog.ITEMSANDDETAILS));
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_Cells, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/cells.png",  string.Empty, string.Empty));
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_LoadingUnits, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/loading_unit.png",  string.Empty, string.Empty));
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_Compartments, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_view_compact_grey_24dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(catalogMenu);

      var configMenu = new MainMenuItem(Resources.Resources.Nav_Lists, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/action/ic_list_white_24dp.png",  string.Empty, string.Empty);
      configMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Lists_Lists, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_list_grey_24dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(configMenu);

      var otherMenu = new MainMenuItem(Resources.Resources.Nav_Others, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/editor/ic_functions_white_24dp.png",  string.Empty, string.Empty);
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_UnderStock_Items, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_trending_down_grey_24dp.png",  string.Empty, string.Empty));
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_CompartmentsMapping, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/compartments_mapping.png",  string.Empty, string.Empty));
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_Inventory, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/inventory.png",  string.Empty, string.Empty));
      this.menu.Items.Add(otherMenu);

      var importExportMenu = new MainMenuItem(Resources.Resources.Nav_ImportExport, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/communication/ic_import_export_white_24dp.png",  string.Empty, string.Empty);
      importExportMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_ImportExport_Import, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_arrow_upward_grey_24dp.png",  string.Empty, string.Empty));
      importExportMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_ImportExport_Export, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_arrow_downward_grey_24dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(importExportMenu);

      var configurationMenu = new MainMenuItem(Resources.Resources.Nav_Configuration, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/action/ic_settings_white_24dp.png",  string.Empty, string.Empty);
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_MeasuringUnits, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_AbcClasses, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_CellStatuses, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_LoadingUnitStatuses, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_CompartmentTypes, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_CompartmentStatuses, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_PackageTypes, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_MaterialStatuses, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_ListTypes, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_ListStatuses, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_ListRowStatuses, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      configurationMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Configuration_MachineTypes, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Ferretto/Menu/grey/ic_build_grey_24dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(configurationMenu);
    }
  }
}
