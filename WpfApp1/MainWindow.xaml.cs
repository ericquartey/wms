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
using Ferretto.VW.InverterDriver;
using Ferretto.VW.ActionBlocks;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InverterDriver inverterDriver;
        private CalibrateVerticalAxis calibrateVA;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            // At this time 1 is the default value for each variable
            int m = 1;
            short ofs = 1;
            short vFast = 1;
            short vCreep = 1;

            this.inverterDriver = new InverterDriver();
            this.calibrateVA = new CalibrateVerticalAxis();
            this.calibrateVA.SetInverterDriverInterface = this.inverterDriver;
            this.calibrateVA.SetVAxisOrigin(m, ofs, vFast, vCreep);


        }
}
}
