using System.Windows;
using DevExpress.Xpf.Core;

namespace Ferretto.WMS.App.Compartment
{
    public partial class App : Application
    {
        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DXSplashScreen.Show<Common.Controls.SplashScreen>();

            new Bootstrapper().Run();
        }

        #endregion Methods
    }
}
