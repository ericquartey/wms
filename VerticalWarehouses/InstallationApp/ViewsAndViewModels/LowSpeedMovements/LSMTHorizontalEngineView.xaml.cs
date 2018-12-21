using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for LSMTHorizontalEngineView.xaml
    /// </summary>
    public partial class LSMTHorizontalEngineView : BaseView
    {
        #region Constructors

        public LSMTHorizontalEngineView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.LSMTHorizontalEngineVMInstance;
        }

        #endregion Constructors
    }
}
