using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for CellsControlView.xaml
    /// </summary>
    public partial class CellsControlView : UserControl
    {
        #region Constructors

        public CellsControlView()
        {
            this.InitializeComponent();
            this.DataContext = new CellsControlViewModel();
        }

        #endregion Constructors
    }
}
