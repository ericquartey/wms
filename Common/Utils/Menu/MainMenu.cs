using System.Collections.Generic;

namespace Ferretto.Common.Utils.Menu
{
  public class AppMenu
  {
    private MainMenu menu;
    public MainMenu Menu
    {
        get { return this.menu; }
    }

    public AppMenu()
    {
      this.Initialize();
    }

    private void Initialize()
    {
      var bgColor = "#525252";
      this.menu = new MainMenu();
      this.menu.Items = new List<MainMenuItem>();
      
      var fileMenu = new MainMenuItem(Resources.Resources.Nav_File, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty);
      fileMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_File_Import, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      fileMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_File_Export, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      fileMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_File_Print, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      fileMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_File_Exit, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(fileMenu);

      var catalogMenu = new MainMenuItem(Resources.Resources.Nav_Catalog, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty);
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_Items, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_black_18dp.png",  nameof(Modules.Catalog), Modules.Catalog.ITEMSANDDETAILS));
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_Cells, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_black_18dp.png",  string.Empty, string.Empty));
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_LoadingUnits, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_black_18dp.png",  string.Empty, string.Empty));
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_Compartments, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_black_18dp.png",  string.Empty, string.Empty));
      catalogMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Catalog_DefaultLoadingUnits, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_black_18dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(catalogMenu);

      var configMenu = new MainMenuItem(Resources.Resources.Nav_Lists, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty);
      configMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Lists_Lists, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      configMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_PackingLists, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(configMenu);

      var otherMenu = new MainMenuItem(Resources.Resources.Nav_Others, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty);
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_UnderStock_Items, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_Reserved_Compartments, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_Items_Compacting, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_Inventory, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_Aisles_Status, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      otherMenu.Children.Add(new MainMenuItem(Resources.Resources.Nav_Others_Loading_Units_Saturation, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
      this.menu.Items.Add(otherMenu);

      this.menu.Items.Add(new MainMenuItem(Resources.Resources.Nav_Help, bgColor, "pack://application:,,,/Ferretto.WMS.Themes;component/Icons/Material/notification/ic_adb_white_18dp.png",  string.Empty, string.Empty));
    }
  }
}
