using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Installer.ViewModels;

namespace Ferretto.VW.Installer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

#if !DEBUG
            this.Top = 0;
            this.Left = 0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
#else
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.MouseDown += this.Shell_MouseDown;
#endif
            this.Loaded += this.OnMainWindowLoaded;
        }

        private void Shell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            viewModel.StartInstallation();
        }


        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            var viewModel = new MainViewModel();
            this.DataContext = viewModel;
        }

    }
}
