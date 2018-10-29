using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesControl
{
    /// <summary>
    /// Interaction logic for GatesControlNavigationButtonsView.xaml
    /// </summary>
    public partial class GatesControlNavigationButtonsView : UserControl
    {
        #region Constructors

        public GatesControlNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = new GatesControlNavigationButtonsViewModel();
        }

        #endregion Constructors
    }
}
