using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    /// <summary>
    /// Interaction logic for SSCradleView.xaml
    /// </summary>
    public partial class SSCradleView : BaseView
    {
        #region Constructors

        public SSCradleView()
        {
            this.InitializeComponent();
            this.DataContext = new SSCradleViewModel();
        }

        #endregion Constructors
    }
}
