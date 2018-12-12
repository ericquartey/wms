using System;
using System.Windows;
using Ferretto.VW.ActionBlocks.Source;
using Ferretto.VW.InverterDriver.Source;

namespace Ferretto.VW.VWApp
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        #endregion Constructors

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            if (ActionManager.PositioningDrawerInstance != null)
            {
                ActionManager.PositioningDrawerInstance.Terminate();
            }
            if (ActionManager.CalibrateVerticalAxisInstance != null)
            {
                ActionManager.CalibrateVerticalAxisInstance.Terminate();
            }
            if (InverteDriverManager.InverterDriverStaticInstance != null)
            {
                InverteDriverManager.InverterDriverStaticInstance.Terminate();
            }
            Application.Current.Shutdown();
            base.OnClosed(e);
        }

        #endregion Methods
    }
}
