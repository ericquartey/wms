using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using Ferretto.Common.Controls.Services;

namespace Ferretto.Common.Controls
{
    public partial class SplashScreen : UserControl
    {
        #region Fields

        private static readonly TimeSpan TimerInterval = new TimeSpan(0, 0, 0, 0, 200);
        private DateTime startTime;

        #endregion Fields

        #region Constructors

        public SplashScreen()
        {
            this.InitializeComponent();

            this.Footer_Text.Text = SplashScreenService.Copyright;

#if DEBUG
            this.SetupTimer();
#endif
        }

        #endregion Constructors

        #region Methods

        private void SetupTimer()
        {
            this.startTime = Process.GetCurrentProcess().StartTime;
            var timer = new DispatcherTimer();
            timer.Interval = TimerInterval;
            timer.Tick += this.Timer_Tick;

            timer.Start();
        }

        private void Timer_Tick(Object sender, EventArgs e)
        {
            var elapsedSeconds = (DateTime.Now - this.startTime).TotalSeconds;
            this.Timing.Text = $"{Common.Resources.DesktopApp.ElapsedTime}: {elapsedSeconds.ToString("#")}s";
        }

        #endregion Methods
    }
}
