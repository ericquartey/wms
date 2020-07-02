using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Layout
{
    public class PresentationService : BasePresentationViewModel
    {
        #region Fields

        private const int SCREENSHOTDELAY = 1000;

        private readonly IBayManager bayManagerService;

        private readonly INavigationService navigationService;

        private readonly ITelemetryHubClient telemetryHubClient;

        private BayNumber bayNumber;

        private bool isScreenCast;

        private bool isServiceOptionsVisible;

        private DelegateCommand screenCastCommand;

        private DelegateCommand sendLogCommand;

        private DelegateCommand sendScreenSnapshotCommand;

        private string userName;

        #endregion

        #region Constructors

        public PresentationService(
            INavigationService navigationService,
            IBayManager bayManagerService,
            ITelemetryHubClient telemetryHubClient)
            : base(PresentationTypes.Service)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(telemetryHubClient));
            this.telemetryHubClient = telemetryHubClient ?? throw new ArgumentNullException(nameof(telemetryHubClient));
        }

        #endregion

        #region Properties

        public bool IsScreenCast
        {
            get => this.isScreenCast;
            set
            {
                if (this.SetProperty(ref this.isScreenCast, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsServiceOptionsVisible
        {
            get => this.isServiceOptionsVisible;
            set
            {
                if (this.SetProperty(ref this.isServiceOptionsVisible, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ScreenCastCommand =>
             this.screenCastCommand
             ??
             (this.screenCastCommand = new DelegateCommand(async () => await this.ScreenCastAsync()));

        public ICommand SendLogCommand =>
                     this.sendLogCommand
             ??
             (this.sendLogCommand = new DelegateCommand(async () => await this.SendLogAsync()));

        public ICommand SendScreenSnapshotCommand =>
                            this.sendScreenSnapshotCommand
            ??
            (this.sendScreenSnapshotCommand = new DelegateCommand(async () => await this.SendScreenSnapshotAsync(), this.CanSendScreenSnapshotAsync));

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.IsServiceOptionsVisible = !this.IsServiceOptionsVisible;

            return Task.CompletedTask;
        }

        public override async Task OnLoadedAsync()
        {
            try
            {
                var bay = await this.bayManagerService.GetBayAsync();
                this.bayNumber = bay.Number;
            }
            catch
            {
                // TODO please fix this
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.sendScreenSnapshotCommand.RaiseCanExecuteChanged();
            base.RaiseCanExecuteChanged();
        }

        private bool CanSendScreenSnapshotAsync()
        {
            return !this.isScreenCast;
        }

        private async Task ScreenCastAsync()
        {
            this.IsServiceOptionsVisible = false;

            if (this.isScreenCast)
            {
                this.IsScreenCast = false;
                return;
            }

            this.IsScreenCast = true;

            Task.Run(async () =>
            {
                do
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                         {
                             try
                             {
                                 var screenshot = this.navigationService.GetScreenshot();
                                 await this.telemetryHubClient.SendScreenshotAsync(this.bayNumber, screenshot);
                             }
                             catch (Exception ex)
                             {
                             }
                         });
                    await Task.Delay(SCREENSHOTDELAY);
                }
                while (this.isScreenCast);
            });
        }

        private async Task SendLogAsync()
        {
            this.IsServiceOptionsVisible = false;
            await this.telemetryHubClient.SendLogsAsync(this.bayNumber);
        }

        private async Task SendScreenSnapshotAsync()
        {
            this.IsServiceOptionsVisible = false;
            var screenshot = this.navigationService.GetScreenshot();
            await this.telemetryHubClient.SendScreenshotAsync(this.bayNumber, screenshot);
        }

        #endregion
    }
}
