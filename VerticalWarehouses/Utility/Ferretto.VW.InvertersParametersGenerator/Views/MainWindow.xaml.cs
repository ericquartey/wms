using System.Windows;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.ViewModels;

namespace Ferretto.VW.InvertersParametersGenerator.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.MouseDown += this.Shell_MouseDown;

            this.Loaded += this.OnMainWindowLoaded;
        }

        #endregion

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            var viewModel = new MainViewModel();
            this.DataContext = viewModel;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            viewModel.StartInstallation();
        }

        private void Shell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        #endregion
    }
}
