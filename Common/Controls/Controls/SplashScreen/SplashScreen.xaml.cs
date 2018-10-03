using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

            this.Footer_Text.Text = this.GetCopyright();
        }

        #endregion Constructors

        #region Methods

        private void SetupTimer()
        {
            this.startTime = Process.GetCurrentProcess().StartTime;
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += this.Timer_Tick;

            timer.Start();
        }

        private string GetCopyright()
        {
            return (this.GetType().Assembly.GetCustomAttributes(false)
                .FirstOrDefault(attribute => attribute is AssemblyCopyrightAttribute) as AssemblyCopyrightAttribute)?.Copyright;
        }

        private void Timer_Tick(Object sender, EventArgs e)
        {
            this.Timing.Text = $"Elapsed Time: {(DateTime.Now - this.startTime).TotalSeconds.ToString("#.0")}s";
        }

        #endregion Methods
    }
}
