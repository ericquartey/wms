using System.Threading;
using System.Windows;
using NLog;

namespace Ferretto.VW.Installer
{
    public partial class App
    {
        #region Fields

        private readonly Logger logger;

        #endregion

        #region Constructors

        public App()
        {
            this.logger = LogManager.GetCurrentClassLogger();

            this.logger.Info("Starting installer application.");

            Application.Current.Exit += this.OnApplicationExit;
        }

        #endregion

        #region Methods

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            this.logger.Info("Exiting installer application.");
        }

        #endregion
    }
}
