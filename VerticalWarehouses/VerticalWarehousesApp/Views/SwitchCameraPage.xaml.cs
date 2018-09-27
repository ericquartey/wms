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

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    /// <summary>
    /// Interaction logic for SwitchCamera.xaml
    /// </summary>
    public partial class SwitchCameraPage : Page
    {
        #region Fields

        private string drawer;
        private MainWindow window;

        #endregion Fields

        #region Constructors

        public SwitchCameraPage(MainWindow window, String drawer)
        {
            this.InitializeComponent();
            this.window = window;
            this.drawer = drawer;
        }

        #endregion Constructors

        #region Methods

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            this.window.NavigateToManualPage(this.drawer);
        }

        private void Slider_ValueChanged(Object sender, RoutedPropertyChangedEventArgs<Double> e)
        {
        }

        #endregion Methods
    }
}
