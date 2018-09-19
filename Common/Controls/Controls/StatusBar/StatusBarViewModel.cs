using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class StatusBarViewModel : BindableBase
    {
        #region Fields

        private const int timeToKeepText = 15;
        private string info;
        private System.Windows.Threading.DispatcherTimer keepInfoTimer = new System.Windows.Threading.DispatcherTimer();

        #endregion Fields

        public StatusBarViewModel()
        {
            this.keepInfoTimer.Tick += new EventHandler(this.keepInfoTimer_Tick);
            this.keepInfoTimer.Interval = new TimeSpan(0, 0, timeToKeepText);

            ServiceLocator.Current.GetInstance<IEventService>()
               .Subscribe((StatusEvent eventArgs) =>
               {
                   this.keepInfoTimer.Stop();
                   this.Info = eventArgs.Info;
                   this.keepInfoTimer.Start();
               });
        }

        #region Properties

        public string Info
        {
            get => this.info;
            set => this.SetProperty(ref this.info, value);
        }

        private void keepInfoTimer_Tick(Object sender, EventArgs e)
        {
            this.Info = string.Empty;
            this.keepInfoTimer.Stop();
        }

        #endregion Properties
    }
}
