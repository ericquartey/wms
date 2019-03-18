using Ferretto.Common.Utils.Modules;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.WMS.App
{
    public partial class Shell : WmsWindow
    {
        #region Fields

        private readonly IModuleManager moduleManager;

        private readonly IRegionManager regionManager;

        #endregion

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

            this.moduleManager.LoadModule(nameof(Layout));
            this.moduleManager.LoadModule(nameof(Machines));
            this.moduleManager.LoadModule(nameof(MasterData));
            this.moduleManager.LoadModule(nameof(Scheduler));
        }

        #endregion
    }
}
