using System.Windows;
using System.Windows.Input;
using Prism.Modularity;

namespace Ferretto.VW.App
{
    public partial class Shell : Window
    {
        #region Constructors

        public Shell()
        {
            this.InitializeComponent();
#if DEBUG
            this.MouseDown += this.Shell_MouseDown;
#endif
        }

        private void Shell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        public Shell(IModuleManager moduleManager)
          : this()
        {
            if (moduleManager is null)
            {
                throw new System.ArgumentNullException(nameof(moduleManager));
            }

            moduleManager.LoadModule(nameof(Utils.Modules.Layout));
            moduleManager.LoadModule(nameof(Utils.Modules.Login));
        }

        #endregion
    }
}
