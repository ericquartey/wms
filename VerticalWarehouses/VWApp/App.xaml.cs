using System.Windows;

namespace Ferretto.VW.VWApp
{
    public partial class App : Application
    {//Instances of differents' projects MainWindow are declared in App.xaml.cs, so that there will be no running processes after closing VWApp.
        #region Properties

        public InstallationApp.MainWindow InstallationAppMainWindow { get; set; }
        public OperatorApp.MainWindow OperatorAppMainWindow { get; set; }

        #endregion Properties
    }
}
