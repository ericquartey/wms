using System.Threading.Tasks;
using System.Windows;

namespace Ferretto.VW.Installer
{
    public partial class MainWindow
    {
        #region Constructors

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
#endif
            this.Loaded += this.OnMainWindowLoaded;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainWindowViewModel;

            viewModel.StartInstallation();
        }

        #endregion

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            var viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;
        }

        #endregion
    }
}
