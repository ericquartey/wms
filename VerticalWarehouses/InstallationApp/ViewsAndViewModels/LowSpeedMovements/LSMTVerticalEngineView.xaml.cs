using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.LowSpeedMovements
{
    /// <summary>
    /// Interaction logic for LSMTVerticalEngineView.xaml
    /// </summary>
    public partial class LSMTVerticalEngineView : BaseView
    {
        #region Constructors

        public LSMTVerticalEngineView()
        {
            this.InitializeComponent();
            this.DataContext = new LSMTVerticalEngineViewModel();
        }

        #endregion Constructors
    }
}
