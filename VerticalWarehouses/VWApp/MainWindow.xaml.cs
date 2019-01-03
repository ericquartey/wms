using System;
using System.Windows;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.VWApp
{
    public partial class MainWindow : Window, IMainWindow
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).DrawerWeightDetection?.Terminate();
            ((MainWindowViewModel)this.DataContext).PositioningDrawer?.Terminate();
            ((MainWindowViewModel)this.DataContext).CalibrateVerticalAxis?.Terminate();
            ((MainWindowViewModel)this.DataContext).Inverter?.Terminate();
            Application.Current.Shutdown();
            base.OnClosed(e);
        }

        #endregion Methods
    }
}
