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

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Picking
{
    /// <summary>
    /// Interaction logic for PickingNavigationButtonView.xaml
    /// </summary>
    public partial class PickingNavigationButtonView : UserControl
    {
        #region Constructors

        public PickingNavigationButtonView()
        {
            this.InitializeComponent();
            this.DataContext = new PickingNavigationButtonViewModel();
        }

        #endregion Constructors
    }
}
