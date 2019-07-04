using System.Configuration;
using System.Windows;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App
{
    public partial class App : WmsApplication
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

        protected override void OnInitialized()
        {
            try
            {
                SplashScreenService.SetMessage(DesktopApp.InitializingLogin);
                SplashScreenService.Hide();

                var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                navigationService.Appear(nameof(Common.Utils.Modules.Layout), Common.Utils.Modules.Layout.LOGINVIEW);

                var assembly = typeof(App).Assembly;
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                this.logger.Info($"Starting application, version '{versionInfo.ProductVersion}'.");
            }
            catch (System.Exception ex)
            {
                this.logger.Error(ex, "An error occurred on application startup.");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.SetLanguage();
            SplashScreenService.Show();
            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            this.logger.Error(e.ExceptionObject as System.Exception, "An unhandled exception was thrown.");
        }

        private void SetLanguage()
        {
            var defaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"];
            if (!string.IsNullOrWhiteSpace(defaultLanguage)
                &&
                System.Globalization.CultureInfo.CurrentUICulture.Name != defaultLanguage)
            {
                this.logger.Info(
                    $"Overriding user's UI language '{System.Globalization.CultureInfo.CurrentUICulture.Name}' with '{defaultLanguage}' as specified in configuration.");

                System.Globalization.CultureInfo.CurrentUICulture =
                    System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);
                System.Globalization.CultureInfo.CurrentCulture =
                    System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);
            }
        }

        #endregion
    }
}
