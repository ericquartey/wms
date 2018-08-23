using System.Windows;
using DevExpress.Xpf.Core;

namespace Ferretto.WMS.App
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      DXSplashScreen.Show<Common.Controls.SplashScreen>();

      var bootstrapper = new Bootstrapper();
      bootstrapper.Run();
    }
  }
}
