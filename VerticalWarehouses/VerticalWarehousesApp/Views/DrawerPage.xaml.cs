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
using Ferretto.VW.VerticalWarehousesApp.ViewModels;

namespace Ferretto.VW.VerticalWarehousesApp.Views
{
    /// <summary>
    /// Interaction logic for DrawerPage.xaml
    /// </summary>
    public partial class DrawerPage : Page
    {
        #region Fields

        private string[] misureCassetto = {
            "1950x650", "1950x840", "1950x1030",
            "2450x650", "2450x840", "2450x1030",
            "3050x650", "3050x840", "3050x1030",
            "3650x650", "3650x840", "3650x1030",
            "4250x650", "4250x840", "4250x1030"
        };

        private DrawerViewModel mwvm;
        private MainWindow window;

        #endregion Fields

        #region Constructors

        public DrawerPage(MainWindow mainWindow)
        {
            this.InitializeComponent();
            this.InizializzaPagina();
            this.window = mainWindow;
        }

        #endregion Constructors

        #region Methods

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            if (this.sel_drawer.SelectedItem != null)
            {
                this.NavigateToSwitchPage();
            }
            else
            {
                MessageBox.Show("Errore: non è stato selezionato nessun cassetto", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
        }

        private void InizializzaPagina()
        {
            for (int i = 0; i < this.misureCassetto.Length; i++)
            {
                string misure_s;
                //if (i < 3)
                //{
                //    misure_s = this.misure[0];
                //}
                //else
                //{
                //    misure_s = i >= 3 && i <= 6 ? this.misure[1] : this.misure[2];
                //}
                this.sel_drawer.Items.Add($"Cassetto {i}, {this.misureCassetto[i]}");
            }
        }

        private void NavigateToSwitchPage()
        {
            var drawerSel = (string)this.sel_drawer.SelectedItem;

            this.window.NavigateToSwitchPage(drawerSel);
        }

        private void VMLoaded(object sender, RoutedEventArgs e)
        {
            this.mwvm = new DrawerViewModel();
        }

        #endregion Methods
    }
}
