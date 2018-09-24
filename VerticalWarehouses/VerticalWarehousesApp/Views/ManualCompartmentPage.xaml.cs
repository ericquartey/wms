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

    public class Item
    {
        public string Name { get; set; }
        public string SurName { get; set; }
    }
    /// <summary>
    /// Interaction logic for PageCompartmentManual.xaml
    /// </summary>
    public partial class ManualCompartmentPage : Page
    {
        private MainWindow window;

        public ManualCompartmentPage(MainWindow mainWindow)
        {
            this.InitializeComponent();
            this.InizializzaPagina();
            this.window = mainWindow;
        }

        private void InizializzaPagina()
        {
            List<Item> items = new List<Item>();
            items.Add(new Item() { Name = "Alice", SurName = "Orsetto" });
            items.Add(new Item() { Name = "Clark", SurName = "Kent" });
            items.Add(new Item() { Name = "Bruce", SurName = "Wayne" });
            this.items_list.ItemsSource = items;

        }

        private void TextBox_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(Object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {

        }

        private void width_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void height_TextChanged(Object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(Object sender, RoutedEventArgs e)
        {

        }

        private void Draw_Click_1(Object sender, RoutedEventArgs e)
        {
            this.NavigateToDrawGridPage();
        }

        private void NavigateToDrawGridPage()
        {
            this.window.NavigateToDrawGridPage();
        }

        private void Button_Click_2(Object sender, RoutedEventArgs e)
        {

        }
    }
}
