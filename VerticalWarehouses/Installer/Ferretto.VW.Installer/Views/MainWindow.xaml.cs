using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.ViewModels;

namespace Ferretto.VW.Installer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.LockWindowPosition();

            this.Loaded += this.OnMainWindowLoaded;
        }

        private void LockWindowPosition()
        {
#if !DEBUG
            this.Top = 0;
            this.Left = 0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
#else
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.MouseDown += this.Shell_MouseDown;
#endif
        }

#if DEBUG

        private void Shell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

#endif

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainWindowViewModel;

            viewModel.StartInstallation();
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            this.DataContext = new MainWindowViewModel();
        }
    }
}
