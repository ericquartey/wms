using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for GetLocalIpView.xaml
    /// </summary>
    public partial class GetLocalIpView : UserControl
    {
        #region Constructors

        public GetLocalIpView()
        {
            this.InitializeComponent();
            this.DataContext = new GetLocalIPViewModel();
        }

        #endregion Constructors
    }
}
