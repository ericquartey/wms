using System.Configuration;
using System.Windows;
using Ferretto.Common.Controls.Services;

namespace Ferretto.WMS.App
{
    public partial class App : Application
    {
        #region Fields

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public App()
        {
            System.AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        }

        #endregion

        #region Methods

        protected override void OnExit(ExitEventArgs e)
        {
            this.logger.Trace("Closing application.");

            base.OnExit(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S2221",
            Justification = "This method log all startup errors")]
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var assembly = typeof(App).Assembly;
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                this.logger.Info($"Starting application, version '{versionInfo.ProductVersion}'.");

                this.SetLanguage();

                SplashScreenService.Show();

                new Bootstrapper().Run();
            }
            catch (System.Exception ex)
            {
                this.logger.Error(ex, "An error occurred on application startup.");
            }
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as System.Exception, "An unhandled exception was thrown.");
        }

        private void SetLanguage()
        {
            var defaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"];
            if (string.IsNullOrWhiteSpace(defaultLanguage) == false
                &&
                System.Globalization.CultureInfo.CurrentUICulture.Name != defaultLanguage)
            {
                this.logger.Info(
                    $"Overriding user's UI language '{System.Globalization.CultureInfo.CurrentUICulture.Name}' with '{defaultLanguage}' as specified in configuration.");

                System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);
            }
        }

        #endregion
    }
}
