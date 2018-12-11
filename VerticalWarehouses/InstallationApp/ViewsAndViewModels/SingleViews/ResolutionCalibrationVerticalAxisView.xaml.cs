using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for ResolutionCalibrationVerticalAxisView.xaml
    /// </summary>
    public partial class ResolutionCalibrationVerticalAxisView : UserControl
    {
        #region Constructors

        public ResolutionCalibrationVerticalAxisView(InverterDriver.InverterDriver driver)
        {
            this.InitializeComponent();

            // ope file and read the resolution parameter

            var resol = new ResolutionCalibrationVerticalAxisViewModel();
            resol.SetInverterDriver = driver;
            //this.DataContext = new ResolutionCalibrationVerticalAxisViewModel();
            this.DataContext = resol;

            // ope file, read the new resolution parameter and save it 

        }

        #endregion Constructors
    }
}
