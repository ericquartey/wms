using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for VerticalOffsetCalibrationView.xaml
    /// </summary>
    public partial class VerticalOffsetCalibrationView : UserControl
    {
        #region Constructors

        public VerticalOffsetCalibrationView()
        {
            this.InitializeComponent();
            this.DataContext = new VerticalOffsetCalibrationViewModel();
        }

        #endregion Constructors
    }
}
