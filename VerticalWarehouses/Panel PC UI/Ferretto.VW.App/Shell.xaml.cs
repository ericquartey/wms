using System;
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
            this.MouseDoubleClick += this.Shell_MouseDoubleClick;
#endif
        }

        private void Shell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Left = 0;
                this.Top = 0;
            }
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
            _ = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));

            moduleManager.LoadModule(nameof(Utils.Modules.Layout));
            moduleManager.LoadModule(nameof(Utils.Modules.Login));
        }

        #endregion
    }
}
