using System.Windows;
using NLog.Time;

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
        }

        #endregion

        #region Methods

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);

            this.DataContext = new MainWindowViewModel();
        }

        #endregion
    }
}
