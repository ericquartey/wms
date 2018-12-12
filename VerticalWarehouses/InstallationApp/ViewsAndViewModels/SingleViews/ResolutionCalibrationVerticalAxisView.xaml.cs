using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for ResolutionCalibrationVerticalAxisView.xaml
    /// </summary>
    public partial class ResolutionCalibrationVerticalAxisView : UserControl
    {
        #region Constructors

        public ResolutionCalibrationVerticalAxisView()
        {
            this.InitializeComponent();
            this.DataContext = new ResolutionCalibrationVerticalAxisViewModel();
        }

        #endregion Constructors
    }
}
