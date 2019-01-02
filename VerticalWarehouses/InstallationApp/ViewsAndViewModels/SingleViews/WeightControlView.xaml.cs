using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for WeightControlView.xaml
    /// </summary>
    public partial class WeightControlView : UserControl
    {
        #region Constructors

        public WeightControlView()
        {
            this.InitializeComponent();
            this.DataContext = ViewModels.WeightControlVMInstance;
        }

        #endregion Constructors
    }
}
