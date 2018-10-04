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

            SplashScreenService.Show();

            new Bootstrapper().Run();
        }

        #endregion Methods
    }
}
