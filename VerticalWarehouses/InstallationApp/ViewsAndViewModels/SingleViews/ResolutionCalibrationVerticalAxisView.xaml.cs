using System.Windows.Controls;

namespace Ferretto.VW.InstallationApp
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
        }

        #endregion

        #region Methods

        private void CustomInputFieldControlFocusable_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            // HACK is not known yet the reason of this method's existance and why it's used in the auto-generated code-behind
        }

        #endregion
    }
}
