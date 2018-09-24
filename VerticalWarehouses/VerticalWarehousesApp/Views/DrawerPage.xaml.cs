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
    /// Interaction logic for DrawerPage.xaml
    /// </summary>
    public partial class DrawerPage : Page
    {
        private MainWindow window;
        public DrawerPage(MainWindow mainWindow)
        {
            this.InitializeComponent();
            this.InizializzaPagina();
            this.window = mainWindow;
        }

        private void InizializzaPagina()
        {
            for(int i = 0; i < 10; i++)
            {
                this.sel_drawer.Items.Add($"Cassetto {i} 50x20");

            }
            
        }
        #region input_control
        private void ComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigateToManualPage();
        }
        #endregion

        #region navigation
        private void NavigateToManualPage()
        {
            this.window.NavigateToManualPage();
        }
        #endregion
    }
}
