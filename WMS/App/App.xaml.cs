using System.Configuration;
using System.Windows;
using Ferretto.Common.Controls.Services;

namespace Ferretto.WMS.App
{
    public partial class App : Application
    {
        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var defaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"] ?? "en";
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(defaultLanguage);

            SplashScreenService.Show();

            new Bootstrapper().Run();
        }

        #endregion Methods
    }
}
