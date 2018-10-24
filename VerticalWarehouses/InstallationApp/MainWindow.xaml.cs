using System.Windows;
using Ferretto.VW.Navigation;
using System.Windows.Input;

namespace Ferretto.VW.InstallationApp
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            NavigationService.BackToVWAppEventHandler += this.HideThisMainWindow;
            this.DataContext = new MainWindowViewModel();
        }

        #endregion Constructors

        #region Methods

        public void BackToVWAppButtonMethod(object sender, MouseButtonEventArgs e)
        {
            this.BackToVWApp();
        }

        public void InitializeNavigation()
        {
            NavigationService.BackToVWAppEventHandler += this.HideThisMainWindow;
        }

        private void BackToVWApp()
        {
            NavigationService.RaiseBackToVWAppEvent();
        }

        private void HideThisMainWindow()
        {
            this.Hide();
        }

        #endregion Methods
    }
}
