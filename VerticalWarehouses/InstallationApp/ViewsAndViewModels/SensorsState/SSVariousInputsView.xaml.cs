using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    /// <summary>
    /// Interaction logic for SSVariousInputsView.xaml
    /// </summary>
    public partial class SSVariousInputsView : UserControl
    {
        #region Constructors

        public SSVariousInputsView()
        {
            this.InitializeComponent();
            this.DataContext = new SSVariousInputsViewModel();
        }

        #endregion Constructors
    }
}
