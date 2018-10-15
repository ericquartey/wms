using System;
using System.Windows.Controls;
using Ferretto.VW.Navigation;
using Ferretto.VW.InstallationApp.ViewModels;
using System.Windows;

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
