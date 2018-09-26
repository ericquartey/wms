using DevExpress.Xpf.Core;
using Ferretto.Common.Utils.Modules;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.App
{
    public partial class Shell : DXWindow
    {
        private readonly IModuleManager moduleManager;
        private readonly IRegionManager regionManager;

        public Shell()
        {
            this.InitializeComponent();

            this.Loaded += this.Shell_Loaded;
        }

        private void Shell_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            DXSplashScreen.Close();
        }

        public Shell(IModuleManager moduleManager, IRegionManager regionManager)
            : this()
        {
            this.moduleManager = moduleManager;
            this.regionManager = regionManager;

            // Load the root module of the application
            this.moduleManager.LoadModule(nameof(Layout));
            this.moduleManager.LoadModule(nameof(MasterData));
        }
    }
}
