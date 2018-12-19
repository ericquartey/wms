using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for CellsPanelsControlView.xaml
    /// </summary>
    public partial class CellsPanelsControlView : UserControl
    {
        #region Constructors

        public CellsPanelsControlView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.CellsPanelControlVMInsance;
        }

        #endregion Constructors
    }
}
