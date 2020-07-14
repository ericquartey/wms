using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Telemetry.Contracts.Hub;
using NLog;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Layout
{
    public class PresentationService : BasePresentationViewModel
    {
        #region Fields

        private const int SCREENSHOTDELAY = 200;

        private readonly IBayManager bayManagerService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly INavigationService navigationService;

        private readonly ISessionService sessionService;

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

            this.bayNumber = BayNumber.None;
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
             (this.screenCastCommand = new DelegateCommand(async () => await this.ToggleScreenCastAsync()));

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

        public async Task CheckBayNumberAsync()
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

        public override Task ExecuteAsync()
        {
            this.IsServiceOptionsVisible = !this.IsServiceOptionsVisible;

            return Task.CompletedTask;
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

        private async Task ToggleScreenCastAsync()
        {
            this.IsServiceOptionsVisible = false;

            this.IsScreenCast = !this.IsScreenCast;
            if (!this.IsScreenCast)
            {
                return;
            }

            await this.CheckBayNumberAsync();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                do
                {
                    byte[] screenshot = null;
                    Application.Current.Dispatcher.Invoke(() =>
                         {
                             screenshot = this.navigationService.TakeScreenshot();
                         });

                    try
                    {
                        if (screenshot != null)
                        {
                            await this.telemetryHubClient.SendScreenCastAsync((int)this.bayNumber, screenshot, DateTimeOffset.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex);
                    }
                    finally
                    {
                        await Task.Delay(SCREENSHOTDELAY);
                    }
                }
                while (this.isScreenCast);
            });
#pragma warning restore CS4014
        }

        private async Task SendLogAsync()
        {
            this.IsServiceOptionsVisible = false;
            await this.CheckBayNumberAsync();

            try
            {
                //await this.telemetryHubClient.SendErrorLogAsync(
                //new ServiceDesk.Telemetry.ErrorLog
                //{
                //    BayNumber = (int)this.bayNumber,
                //    AdditionalText = "Test",
                //    Code = 14,
                //    OccurrenceDate = DateTimeOffset.Now
                //});

                await this.telemetryHubClient.SendMissionLogAsync(
                new ServiceDesk.Telemetry.MissionLog
                {
                     Bay = (int)this.bayNumber,
                     CellId = 1,
                     Destination = "Test",
                     Direction = 1,
                     MissionType = "Test mission",
                     Priority = 1,
                     Stage = "",
                     Status = "Moving 2",
                     StopReason = 1,
                });

            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private async Task SendScreenSnapshotAsync()
        {
            try
            {
                this.IsServiceOptionsVisible = false;

                await this.CheckBayNumberAsync();
                var screenshot = this.navigationService.TakeScreenshot();
                await this.telemetryHubClient.SendScreenShotAsync((int)this.bayNumber, DateTime.Now, screenshot);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        #endregion
    }
}
