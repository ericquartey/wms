using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    /// <summary>
    /// Interaction logic for VerticalAxisCalibrationView.xaml
    /// </summary>
    public partial class VerticalAxisCalibrationView : BaseView
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
