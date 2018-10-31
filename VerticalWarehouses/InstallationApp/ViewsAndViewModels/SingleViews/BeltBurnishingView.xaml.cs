using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for BeltBurnishingView.xaml
    /// </summary>
    public partial class BeltBurnishingView : UserControl
    {
        #region Constructors

        public BeltBurnishingView()
        {
            this.InitializeComponent();
            this.DataContext = new BeltBurnishingViewModel();
        }

        #endregion Constructors
    }
}
