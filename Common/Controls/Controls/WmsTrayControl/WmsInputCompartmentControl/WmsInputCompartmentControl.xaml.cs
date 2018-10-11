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
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{ 
    /// <summary>
    /// Interaction logic for WmsCompartmentInputControl.xaml
    /// </summary>
    public partial class WmsInputCompartmentControl : UserControl
    {
        public WmsInputCompartmentControl()
        {
            this.InitializeComponent();
            this.DataContext = new WmsInputCompartmentControlViewModel();
        }
        protected void CreateNewCompartmentCommand(object sender, MouseButtonEventArgs e)
        {
            CompartmentDetails compartmentDetails = null;
            EventUI eventNew = new EventUI(sender, e, compartmentDetails);
            var vm = this.DataContext as IEventUI;
            vm.CreateButtonForm(eventNew);
        }

        private void CreateNewCompartmentCommand(Object sender, RoutedEventArgs e)
        {
            CompartmentDetails compartmentDetails = new CompartmentDetails {
                Width =int.Parse(this.WidthText.InnerTextBox.Text),
                Height =int.Parse(this.HeightText.InnerTextBox.Text),
                XPosition = int.Parse(this.PositionXText.InnerTextBox.Text),
                YPosition = int.Parse(this.PositionYText.InnerTextBox.Text)
            };
            EventUI eventNew = new EventUI(sender, e, compartmentDetails);
            var vm = this.DataContext as IEventUI;
            vm.CreateButtonForm(eventNew);
        }
    }
}
