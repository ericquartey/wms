using System.Configuration;
using System.Windows;
using Ferretto.Common.Controls.Services;

namespace Ferretto.WMS.App
{
    public partial class App : Application
    {
        #region Methods

        protected override void OnExit(ExitEventArgs e)
        {
            NLog.LogManager
               .GetCurrentClassLogger()
               .Trace("Closing application.");

            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            NLog.LogManager
              .GetCurrentClassLogger()
              .Info(string.Format("Starting application, version '{0}'.", versionInfo.ProductVersion));

            SetLanguage();

            SplashScreenService.Show();

            new Bootstrapper().Run();
        }

        private static void SetLanguage()
        {
            var defaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"];
            if (string.IsNullOrWhiteSpace(defaultLanguage) == false
                &&
                System.Globalization.CultureInfo.CurrentUICulture.Name != defaultLanguage)
            {
                NLog.LogManager
                  .GetCurrentClassLogger()
                  .Info(string.Format("Overriding user's UI language '{0}' with '{1}' as specified in configuration.",
                    System.Globalization.CultureInfo.CurrentUICulture.Name,
                    defaultLanguage));

                System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);
            }
        }

        #endregion Methods
    }
}
