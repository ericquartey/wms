using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    /// <summary>
    /// Interaction logic for SSProvaView.xaml
    /// </summary>
    public partial class SSProvaView : BaseView
    {
        #region Constructors

        public SSProvaView()
        {
            this.InitializeComponent();
            this.DataContext = new SSProvaViewModel();
        }

        #endregion Constructors
    }
}
