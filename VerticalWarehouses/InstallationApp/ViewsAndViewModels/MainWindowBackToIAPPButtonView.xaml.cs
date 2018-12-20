using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels
{
    /// <summary>
    /// Interaction logic for MainWindowBackToIAPPButtonView.xaml
    /// </summary>
    public partial class MainWindowBackToIAPPButtonView : UserControl
    {
        #region Constructors

        public MainWindowBackToIAPPButtonView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.MainWindowBackToIAPPButtonVMInstance;
        }

        #endregion Constructors
    }
}
