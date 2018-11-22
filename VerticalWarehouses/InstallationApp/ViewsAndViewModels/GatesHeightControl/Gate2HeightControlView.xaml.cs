using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.GatesHeightControl
{
    /// <summary>
    /// Interaction logic for Gate2HeightControlView.xaml
    /// </summary>
    public partial class Gate2HeightControlView : UserControl
    {
        #region Constructors

        public Gate2HeightControlView()
        {
            this.InitializeComponent();
            this.DataContext = new Gate2HeightControlViewModel();
        }

        #endregion Constructors
    }
}
