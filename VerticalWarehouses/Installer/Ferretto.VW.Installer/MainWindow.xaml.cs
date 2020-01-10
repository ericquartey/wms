namespace Ferretto.VW.Installer
{
    public partial class MainWindow
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
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
