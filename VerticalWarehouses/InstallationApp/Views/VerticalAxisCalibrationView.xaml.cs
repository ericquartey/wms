using System.Windows.Controls;
using Ferretto.VW.InstallationApp.ViewModels;

namespace Ferretto.VW.InstallationApp.Views
{
    public partial class VerticalAxisCalibrationView : UserControl
    {
        #region Constructors

        public VerticalAxisCalibrationView()
        {
            this.InitializeComponent();
            this.DataContext = new VerticalAxisCalibrationViewModel();
        }

        #endregion Constructors
    }
}
