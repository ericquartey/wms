using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;

public partial class SplashScreen : UserControl
{
    #region Fields

    private static readonly TimeSpan TimerInterval = new TimeSpan(0, 0, 0, 0, 200);

    private DateTime startTime;

    #endregion

    #region Constructors

    public SplashScreen()
    {
        this.InitializeComponent();

        this.copyrightText.Text = SplashScreenService.Copyright;
        this.versionText.Text = string.Format(VWApp.Version, SplashScreenService.Version);

#if DEBUG
        this.SetupTimer();
#endif
    }

    #endregion

    #region Methods

    private void SetupTimer()
    {
        this.startTime = Process.GetCurrentProcess().StartTime;
        var timer = new DispatcherTimer();
        timer.Interval = TimerInterval;
        timer.Tick += this.Timer_Tick;

        timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        var elapsedSeconds = (DateTime.Now - this.startTime).TotalSeconds;
        this.Timing.Text = $"{DesktopApp.ElapsedTime}: {elapsedSeconds.ToString("#")}s";
    }

    #endregion
}
}
