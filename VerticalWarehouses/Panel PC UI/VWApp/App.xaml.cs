using NLog;

namespace Ferretto.VW.App
{
    public partial class App : System.Windows.Application
    {
        #region Fields

        private Logger logger;

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
            var bootstrapper = new Bootstrapper();

            bootstrapper.Run();
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as System.Exception, "An unhandled exception was thrown.");
        }

        #endregion
    }
}
