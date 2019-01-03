using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for LSMTNavigationButtonsView.xaml
    /// </summary>
    public partial class LSMTNavigationButtonsView : BaseView
    {
        #region Constructors

        public LSMTNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.LSMTNavigationButtonsVMInstance;
        }

        #endregion Constructors
    }
}
