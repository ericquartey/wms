using System;
using System.Configuration;
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

            var fullscreen = true;
            try
            {
                fullscreen = Convert.ToBoolean(ConfigurationManager.AppSettings["FullScreen"]);
            }
            catch (Exception)
            {
            }

            //#if DEBUG
            if (!fullscreen)
            {
                //#if DEBUG
                this.MouseDown += this.Shell_MouseDown;
                //#endif
                this.MouseDoubleClick += this.Shell_MouseDoubleClick;
            }
            //#endif
        }

        public Shell(IModuleManager moduleManager)
          : this()
        {
            _ = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));

            moduleManager.LoadModule(nameof(Utils.Modules.Layout));
            moduleManager.LoadModule(nameof(Utils.Modules.Login));
        }

        #endregion

        #region Methods

        private void Shell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var p = e.GetPosition(this);
                if (p.X < 250 && p.Y < 70)
                {
                    this.Left = 0;
                    this.Top = 0;
                }
            }
        }

        //#if DEBUG

        private void Shell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        #endregion

        //#endif
    }
}
