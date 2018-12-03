using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class StatusBarViewModel : Prism.Mvvm.BindableBase
    {
        #region Fields

        private const int timeToKeepText = 15;
        private string info;
        private System.Windows.Threading.DispatcherTimer keepInfoTimer = new System.Windows.Threading.DispatcherTimer();
        private string schedulerStatus;
        private StatusType symbolName;

        #endregion Fields

        #region Constructors

        public StatusBarViewModel()
        {
            this.SchedulerStatus = nameof(Icons.SchedulerOffLine);
            this.keepInfoTimer.Tick += new EventHandler(this.keepInfoTimer_Tick);
            this.keepInfoTimer.Interval = new TimeSpan(0, 0, timeToKeepText);

            ServiceLocator.Current.GetInstance<IEventService>()
               .Subscribe((StatusEventArgs eventArgs) =>
               {
                   this.keepInfoTimer.Stop();
                   this.Info = eventArgs.Message;
                   this.SymbolName = eventArgs.Type;
                   this.SchedulerStatus = eventArgs.IsSchedulerOnline ? nameof(Icons.SchedulerOnLine) : nameof(Icons.SchedulerOffLine);
                   this.keepInfoTimer.Start();
               });
        }

        #endregion Constructors

        #region Properties

        public string Info
        {
            get => this.info;
            set => this.SetProperty(ref this.info, value);
        }

        public string SchedulerStatus
        {
            get => this.schedulerStatus;
            set => this.SetProperty(ref this.schedulerStatus, value);
        }

        public StatusType SymbolName
        {
            get => this.symbolName;
            set => this.SetProperty(ref this.symbolName, value);
        }

        #endregion Properties

        #region Methods

        private void keepInfoTimer_Tick(Object sender, EventArgs e)
        {
            this.Info = string.Empty;
            this.symbolName = string.Empty;
            this.keepInfoTimer.Stop();
        }

        #endregion Methods
    }
}
