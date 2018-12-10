using System;
using System.Windows;
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
            base.OnClosed(e);
            InverteDriverManager.InverterDriverStaticInstance.Terminate();
            Application.Current.Shutdown();
        }

        #endregion Methods
    }
}
