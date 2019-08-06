using System.Windows;
using Prism.Modularity;

namespace Ferretto.VW.App
{
    public partial class Shell : Window
    {
        #region Constructors

        public Shell()
        {
            this.InitializeComponent();
        }

        public Shell(IModuleManager moduleManager)
          : this()
        {
            moduleManager.LoadModule(nameof(Utils.Modules.Layout));
            moduleManager.LoadModule(nameof(Utils.Modules.Login));
        }

        #endregion
    }
}
