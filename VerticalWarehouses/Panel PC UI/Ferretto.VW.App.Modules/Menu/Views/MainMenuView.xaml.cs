using System;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Menu.Views
{
    public partial class MainMenuView
    {
        #region Fields

        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        #endregion

        #region Constructors

        public MainMenuView()
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
            this.txtTime.Text = string.Format(Localized.Instance.CurrentCulture, "{0:t}", DateTime.Now);
            this.txtDate.Text = string.Format(Localized.Instance.CurrentCulture, "{0:d}", DateTime.Now);
            this.BayBadge.BayNumber = this.BayBadge.BayNumber;
        }

        #endregion
    }
}
