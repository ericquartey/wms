using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Ferretto.Common.Controls
{
    public partial class SplashScreen : UserControl
    {
        #region Fields

        private DateTime startTime;

        #endregion Fields

        #region Constructors

        public SplashScreen()
        {
            this.InitializeComponent();
#if DEBUG
            this.SetupTimer();
#endif
        }

        #endregion Constructors

        #region Methods

        private void SetupTimer()
        {
            this.startTime = DateTime.Now;
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += this.Timer_Tick;

            timer.Start();
        }

        private void Timer_Tick(Object sender, EventArgs e)
        {
            this.Timing.Text = $"Elapsed Time: {(DateTime.Now - this.startTime).TotalSeconds.ToString("#.0")}s";
        }

        #endregion Methods
    }
}
