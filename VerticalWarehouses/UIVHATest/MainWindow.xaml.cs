using System;
using System.Windows;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InverterDriver;

namespace UIVHATest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InverterDriver inverterDriver;
        private CalibrateHorizontalAxis calibrateHorizontalAxis;
        //private DrawerWeightDetection pippo;
        
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void BtnConnectInverter_Click(Object sender, RoutedEventArgs e)
        {
            var success = this.inverterDriver.Initialize();
        }

        private void BtnSend_Click(Object sender, RoutedEventArgs e)
        {

        }
    }
}
