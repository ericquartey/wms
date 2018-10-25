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
            NavigationService.BackToVWAppEventHandler += () => this.Hide();
            this.DataContext = new MainWindowViewModel();
        }

        #endregion Constructors

        #region Methods

        public void BackToVWAppButtonMethod(object sender, MouseButtonEventArgs e)
        {
            NavigationService.RaiseBackToVWAppEvent();
        }

        #endregion Methods
    }
}
