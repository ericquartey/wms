using NLog;

namespace Ferretto.VW.Simulator
{
    public partial class App : System.Windows.Application
    {
        #region Fields

        private readonly Logger logger;

        #endregion

        #region Constructors

        public App()
        {
            this.logger = LogManager.GetCurrentClassLogger();
            System.AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        }

        #endregion

        #region Properties

        public System.Windows.Window InstallationAppMainWindowInstance { get; set; }

        public Prism.Mvvm.BindableBase InstallationAppMainWindowViewModel { get; set; }

        public bool MachineOk { get; set; }

        public System.Windows.Window OperatorAppMainWindowInstance { get; set; }

        #endregion

        #region Methods

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            this.ForceLanguage();

            var bootstrapper = new Bootstrapper();

            bootstrapper.Run();
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as System.Exception, "An unhandled exception was thrown.");
        }

        private void ForceLanguage()
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Default.Culture);
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Settings.Default.Culture);
        }

        #endregion
    }
}
