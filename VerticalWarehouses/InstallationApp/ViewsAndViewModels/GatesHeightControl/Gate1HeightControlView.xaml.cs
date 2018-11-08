using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesHeightControl
{
    /// <summary>
    /// Interaction logic for Gate1HeightControlView.xaml
    /// </summary>
    public partial class Gate1HeightControlView : UserControl
    {
        #region Constructors

        public Gate1HeightControlView()
        {
            this.InitializeComponent();
            this.DataContext = new Gate1HeightControlViewModel();
        }

        #endregion Constructors
    }
}
