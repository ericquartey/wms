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
      base.OnStartup(e);
      var bootstrapper = new Bootstrapper();
      bootstrapper.Run();
    }
  }
}
