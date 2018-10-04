using System.Windows;

namespace Ferretto.WMS.App.Compartment
{
    public partial class App : Application
    {
        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            new Bootstrapper().Run();
        }

        #endregion Methods
    }
}
