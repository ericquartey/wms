using System;

namespace Ferretto.VW.App.Menu.Views
{
    public partial class MaintenanceMenuView
    {
        #region Fields

        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        #endregion

        #region Constructors

        public MaintenanceMenuView()
        {
            this.InitializeComponent();

            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;
            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            this.dispatcherTimer.Start();

            this.DispatcherTimer_Tick(null, null);
        }

        #endregion

        #region Methods

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.txtTime.Text = DateTime.Now.ToShortTimeString();
            this.txtDate.Text = DateTime.Now.ToShortDateString();
        }

        #endregion
    }
}
