using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp
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
            this.DataContext = ViewModels.SSCradleVMInstance;
        }

        #endregion Constructors
    }
}
