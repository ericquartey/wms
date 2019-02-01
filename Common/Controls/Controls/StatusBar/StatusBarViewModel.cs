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

        private readonly System.Windows.Threading.DispatcherTimer keepInfoTimer = new System.Windows.Threading.DispatcherTimer();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string iconName;

        private string message;

        private string schedulerStatus;

        #endregion Fields

        #region Constructors

        public StatusBarViewModel()
        {
            this.SchedulerStatus = nameof(Icons.SchedulerOffLine);
            this.keepInfoTimer.Tick += this.KeepInfoTimer_Tick;
            this.keepInfoTimer.Interval = new TimeSpan(0, 0, timeToKeepText);

            ServiceLocator.Current.GetInstance<IEventService>()
               .Subscribe((StatusPubSubEvent eventArgs) =>
               {
                   if (eventArgs.Exception != null)
                   {
                       this.logger.Error(eventArgs.Exception, "Displaying status error message.");
                   }
                   else
                   {
                       this.logger.Trace(string.Format("Displaying status message '{0}'.", eventArgs.Message));
                   }

                   this.keepInfoTimer.Stop();
                   this.Message = eventArgs.Message;
                   this.IconName = this.GetIconNameFromStatusType(eventArgs.Type);
                   this.SchedulerStatus = eventArgs.IsSchedulerOnline ? nameof(Icons.SchedulerOnLine) : nameof(Icons.SchedulerOffLine);
                   this.keepInfoTimer.Start();
               });
        }

        #endregion Constructors

        #region Properties

        public string IconName
        {
            get => this.iconName;
            set => this.SetProperty(ref this.iconName, value);
        }

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        public string SchedulerStatus
        {
            get => this.schedulerStatus;
            set => this.SetProperty(ref this.schedulerStatus, value);
        }

        #endregion Properties

        #region Methods

        private string GetIconNameFromStatusType(StatusType status)
        {
            return $"{nameof(StatusType)}{status}";
        }

        private void KeepInfoTimer_Tick(object sender, EventArgs e)
        {
            this.Message = string.Empty;
            this.IconName = string.Empty;
            this.keepInfoTimer.Stop();
        }

        #endregion Methods
    }
}
