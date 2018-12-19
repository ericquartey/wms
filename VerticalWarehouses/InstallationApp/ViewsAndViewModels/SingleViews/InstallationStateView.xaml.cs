using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for InstallationStateView.xaml
    /// </summary>
    public partial class InstallationStateView : UserControl
    {
        #region Constructors

        public InstallationStateView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.InstallationStateVMInstance;
        }

        #endregion Constructors
    }
}
