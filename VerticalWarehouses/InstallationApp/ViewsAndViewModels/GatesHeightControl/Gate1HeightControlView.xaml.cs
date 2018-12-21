using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
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
            this.DataContext = ViewModels.Gate1HeightControlVMInstance;
        }

        #endregion Constructors
    }
}
