using System.Windows;
using Ferretto.VW.Navigation;

namespace Ferretto.VW.VWApp
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            NavigationService.BackToVWAppEventHandler += this.RestoreVWAppWindow;
        }

        #endregion Constructors

        #region Methods

        private void RestoreVWAppWindow()
        {
            this.Show();
        }

        #endregion Methods
    }
}
