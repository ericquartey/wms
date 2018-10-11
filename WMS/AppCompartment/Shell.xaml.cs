using DevExpress.Xpf.Core;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.App.Compartment
{
    public partial class Shell : DXWindow
    {
        #region Fields

        private readonly IModuleManager moduleManager;
        private readonly IRegionManager regionManager;

        #endregion Fields

        #region Constructors

        public Shell()
        {
            this.InitializeComponent();
        }

        public Shell(IModuleManager moduleManager, IRegionManager regionManager)
            : this()
        {
            this.moduleManager = moduleManager;
            this.regionManager = regionManager;

            // Load the root module of the application
            this.moduleManager.LoadModule(nameof(Compartment));
        }

        #endregion Constructors
    }
}
