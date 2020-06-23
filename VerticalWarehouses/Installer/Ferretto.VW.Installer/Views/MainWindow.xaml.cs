using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.Services;
using Ferretto.VW.Installer.ViewModels;

namespace Ferretto.VW.Installer.Views
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();

            this.LockWindowPosition();

            this.Loaded += async (sender, e) => await this.OnWindowLoadedAsync();
        }

        #endregion

        #region Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.DataContext = new MainWindowViewModel(
                NavigationService.GetInstance(),
                Container.GetInstallationService(),
                Container.GetSetupModeService(),
                NotificationService.GetInstance());
        }

        private void LockWindowPosition()
        {
#if !DEBUG
            this.WindowState = WindowState.Maximized;

#else
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.MouseDown += this.OnWindowMouseDown;
#endif
        }

        private async Task OnWindowLoadedAsync()
        {
            var viewModel = this.DataContext as IViewModel;

            await viewModel.OnAppearAsync();
        }

#if DEBUG
        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton is MouseButton.Left)
            {
                this.DragMove();
            }
        }
#endif

        #endregion

    }
}
