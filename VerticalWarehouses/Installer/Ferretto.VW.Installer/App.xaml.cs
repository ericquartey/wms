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

#if DEBUG
            System.Globalization.CultureInfo.CurrentUICulture
                = System.Globalization.CultureInfo.CurrentCulture
                = new System.Globalization.CultureInfo("it-IT");
#endif
        }

        #endregion

        #region Methods

        private void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            this.logger.Error("Unhandled exception.");
            this.logger.Error(e.Exception);

#if !DEBUG
            // mark the exception as handled to avoid crashing the application
            e.Handled = true;
#endif
            LogManager.Flush();
        }

        #endregion
    }
}
