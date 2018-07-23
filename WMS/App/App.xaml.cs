using System.Windows;

namespace Ferretto.WMS.App
{
  /// <summary>
  /// Logica di interazione per App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
      base.OnStartup(e);
      Bootstrapper bootstrapper = new Bootstrapper();
      bootstrapper.Run();
    }
  }
}
