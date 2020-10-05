using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Layout.Extensions;
using Ferretto.VW.App.Modules.Layout.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Telemetry.Contracts.Hub;
using Newtonsoft.Json;
using NLog;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout
{
    public class PresentationService : BasePresentationViewModel
    {
        #region Constants

        private const int SCREENSHOTDELAY = 200;

        private const int SCREENCASTDURATION = 30;

        #endregion

        #region Fields

        private const string TelemetryServiceUrlKey = "TelemetryService:Url";

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();

        private readonly IEventAggregator eventAggregator;

        private readonly IBayManager bayManagerService;

        private readonly IDialogService dialogService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly INavigationService navigationService;

        private readonly ISessionService sessionService;

        private readonly ITelemetryHubClient telemetryHubClient;

        private readonly BayNumber bayNumber;

        private bool isScreenCast;

        private bool isServiceOptionsVisible;

        private DelegateCommand screenCastCommand;

        private DelegateCommand saveLogsCommand;

        private DelegateCommand sendScreenSnapshotCommand;

        private string userName;

        #endregion

        #region Constructors

        public PresentationService(
            INavigationService navigationService,
            IBayManager bayManagerService,
            IDialogService dialogService,
            ISessionService sessionService,
            IEventAggregator eventAggregator,
            ITelemetryHubClient telemetryHubClient)
            : base(PresentationTypes.Service)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(telemetryHubClient));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.sessionService = sessionService;
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.telemetryHubClient = telemetryHubClient ?? throw new ArgumentNullException(nameof(telemetryHubClient));

            this.bayNumber = (BayNumber)Enum.Parse(typeof(BayNumber), ConfigurationManager.AppSettings.GetBayNumber());
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

        public ICommand SaveLogsCommand =>
            this.saveLogsCommand
            ??
            (this.saveLogsCommand = new DelegateCommand(async () => await this.SaveLogsAsync()));

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

        protected override void RaiseCanExecuteChanged()
        {
            this.sendScreenSnapshotCommand.RaiseCanExecuteChanged();
            base.RaiseCanExecuteChanged();
        }

        private bool CanSendScreenSnapshotAsync()
        {
            //return false;
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                var screenCastStart = DateTimeOffset.Now;

                do
                {
                    byte[] screenshot = null;
                    Application.Current.Dispatcher.Invoke(() =>
                         {
                             screenshot = this.navigationService.TakeScreenshot(true);
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
                        if (screenCastStart.AddMinutes(SCREENCASTDURATION) < DateTimeOffset.Now)
                        {
                            this.IsScreenCast = false;
                            this.ShowNotification(InstallationApp.ScreenCastDurationExpire);
                        }
                    }
                }
                while (this.IsScreenCast);
            });

#pragma warning restore CS4014
        }

        public void ShowNotification(string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            this.EventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, severity));
        }

        private async Task SaveLogsAsync()
        {
            this.IsServiceOptionsVisible = false;

            try
            {
                var machineLogs = await this.LoadMachineLogsFromWebServiceAsync();
                this.ExportLogsOnFolder(machineLogs);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        public async Task<MachineLogs> LoadMachineLogsFromWebServiceAsync()
        {
            var baseUri = new Uri(ConfigurationManager.AppSettings.Get(TelemetryServiceUrlKey));
            if (baseUri is null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            this.logger.Debug($"Loading logs from telemetryservice'{baseUri}' ...");
            try
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.LoadingLogsFromTelemetryService"), NotificationSeverity.Info);

                var machineLogs = new MachineLogs();

                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(2);

                    var serviceUri = $"{baseUri}api/logs/errors";
                    this.logger.Debug($"Loading error logs from telemetryservice'{baseUri}' ...");
                    var responseContents = await httpClient.GetContentFromServiceAsync(serviceUri);
                    var errorLogs = JsonConvert.DeserializeObject<List<ServiceDesk.Telemetry.ErrorLog>>(responseContents, serializerSettings);

                    serviceUri = $"{baseUri}api/logs/missions";
                    this.logger.Debug($"Loading mission logs from telemetryservice'{baseUri}' ...");
                    responseContents = await httpClient.GetContentFromServiceAsync(serviceUri);
                    var missions = JsonConvert.DeserializeObject<List<ServiceDesk.Telemetry.MissionLog>>(responseContents, serializerSettings);

                    serviceUri = $"{baseUri}api/logs/screenshots";
                    this.logger.Debug($"Loading screenshot logs from telemetryservice'{baseUri}' ...");
                    responseContents = await httpClient.GetContentFromServiceAsync(serviceUri);
                    var screenshots = JsonConvert.DeserializeObject<List<ServiceDesk.Telemetry.ScreenShot>>(responseContents, serializerSettings);

                    machineLogs.ErrorLogs = errorLogs;
                    machineLogs.MissionLogs = missions;
                    machineLogs.ScreenShots = screenshots;

                    this.ShowNotification(Resources.Localized.Get("InstallationApp.LogsFromTelemetryServiceCompleted"), NotificationSeverity.Info);
                };

                this.logger.Debug($"Logs loaded.");

                return machineLogs;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }

            return null;
        }

        public void ShowNotification(Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.logger.Error(exception);

            this.EventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(exception));
        }

        private void ExportLogsOnFolder(MachineLogs machineLogs)
        {
            try
            {
                if (machineLogs is null)
                {
                    return;
                }

                var folder = this.dialogService.BrowseFolder(InstallationApp.SaveLogsFile, "c:");

                if (!string.IsNullOrEmpty(folder))
                {
                    var settings = new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                    };
                    var json = JsonConvert.SerializeObject(machineLogs, settings);
                    var machineSerial = this.sessionService.MachineIdentity.SerialNumber;
                    var newFolder = $"{folder}\\{DateTime.Now:yyyyMMdd_HHmmss}_{machineSerial}";
                    if (!Directory.Exists(newFolder))
                    {
                        Directory.CreateDirectory(newFolder);
                    }
                    var fullPathFileName = $"{newFolder}\\machinelogs.json";
                    File.WriteAllText(fullPathFileName, json);

                    if (machineLogs.ScreenShots != null)
                    {
                        foreach (var screenShot in machineLogs.ScreenShots)
                        {
                            var fullPathImageFileName = $"{newFolder}\\ScreenShot_{screenShot.TimeStamp.ToLocalTime():yyyyMMdd_HHmmss}_Bay{screenShot.BayNumber}.jpg";
                            using (var image = Image.FromStream(new MemoryStream(screenShot.Image)))
                            {
                                image.Save(fullPathImageFileName, ImageFormat.Jpeg);
                            }
                        }
                    }
                    this.ShowNotification(Resources.Localized.Get("InstallationApp.SaveSuccessful"));
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.Message, NotificationSeverity.Error);
            }
        }

        private async Task SendScreenSnapshotAsync()
        {
            try
            {
                this.IsServiceOptionsVisible = false;

                var screenshot = this.navigationService.TakeScreenshot(false);
                await this.telemetryHubClient.SendScreenShotAsync((int)this.bayNumber, DateTimeOffset.Now, screenshot);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        #endregion
    }
}
