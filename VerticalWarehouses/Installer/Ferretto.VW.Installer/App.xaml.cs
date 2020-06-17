using System.Windows;
using NLog;

namespace Ferretto.VW.Installer
{
    public partial class App : Application
    {
        #region Fields

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public App()
        {
            this.DispatcherUnhandledException += this.OnUnhandledException;
        }

        #endregion

        #region Methods

        private void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.Exception);
            LogManager.Flush();
        }

        #endregion
    }
}
