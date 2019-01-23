using System.Windows;
using System.Windows.Input;
using Ferretto.VW.Navigation;

namespace Ferretto.VW.OperatorApp
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
