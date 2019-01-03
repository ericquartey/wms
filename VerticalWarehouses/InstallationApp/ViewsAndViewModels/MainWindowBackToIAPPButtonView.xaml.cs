using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
{
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
