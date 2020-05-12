using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls
{
    public partial class SplashScreen : UserControl
    {
        #region Constructors

        public SplashScreen()
        {
            this.InitializeComponent();

            this.copyrightText.Text = SplashScreenService.Copyright;
            this.versionText.Text = string.Format(General.Version, SplashScreenService.Version);
        }

        #endregion
    }
}
