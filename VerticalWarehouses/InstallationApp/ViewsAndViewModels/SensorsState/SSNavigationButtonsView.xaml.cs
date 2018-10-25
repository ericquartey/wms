using Ferretto.VW.Utils.Source;
using Ferretto.VW.Navigation;
using System.Windows.Input;
using System.Windows;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    /// <summary>
    /// Interaction logic for SSNavigationButtonsView.xaml
    /// </summary>
    public partial class SSNavigationButtonsView : BaseView
    {
        #region Constructors

        public SSNavigationButtonsView()
        {
            this.InitializeComponent();
            this.DataContext = new SSNavigationButtonsViewModel();
        }

        #endregion Constructors
    }
}
