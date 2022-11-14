using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class BaseParameterInverterViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly ISessionService sessionService;

        private SubscriptionToken inverterParameterReceivedToken;

        private SubscriptionToken inverterProgrammingMessageReceivedToken;

        private SubscriptionToken inverterReadingMessageReceivedToken;

        private bool isBusy;

        #endregion

        #region Constructors

        public BaseParameterInverterViewModel(ISessionService sessionService)
                : base(PresentationMode.Installer)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        #endregion

        #region Methods

        public static IEnumerable<FileInfo> FilterInverterConfigurationFile(IEnumerable<FileInfo> fileInfo)
        {
            var fillterFiles = new List<FileInfo>();
            foreach (var file in fileInfo)
            {
                try
                {
                    var json = File.ReadAllText(file.FullName);

                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Inverter>>(json.ToString(),
                        new Newtonsoft.Json.JsonConverter[]
                        {
                            new CommonUtils.Converters.IPAddressConverter(),
                            new Newtonsoft.Json.Converters.StringEnumConverter(),
                        });

                    fillterFiles.Add(file);
                }
                catch (Exception)
                {
                }
            }
            return fillterFiles;
        }

        public static IEnumerable<FileInfo> FilterInverterConfigurationFile(IEnumerable<string> files)
        {
            var fillterFiles = new List<FileInfo>();
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                try
                {
                    var json = File.ReadAllText(fileInfo.FullName);

                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Inverter>>(json.ToString(),
                        new Newtonsoft.Json.JsonConverter[]
                        {
                            new CommonUtils.Converters.IPAddressConverter(),
                            new Newtonsoft.Json.Converters.StringEnumConverter(),
                        });

                    fillterFiles.Add(fileInfo);
                }
                catch (Exception)
                {
                }
            }
            return fillterFiles;
        }

        public bool CanSave()
        {
            return !this.isBusy;
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.inverterReadingMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterReadingMessageData>>().Unsubscribe(this.inverterReadingMessageReceivedToken);
                this.inverterReadingMessageReceivedToken?.Dispose();
                this.inverterReadingMessageReceivedToken = null;
            }

            if (this.inverterProgrammingMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterProgrammingMessageData>>().Unsubscribe(this.inverterProgrammingMessageReceivedToken);
                this.inverterProgrammingMessageReceivedToken?.Dispose();
                this.inverterProgrammingMessageReceivedToken = null;
            }

            if (this.inverterParameterReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterParametersMessageData>>().Unsubscribe(this.inverterParameterReceivedToken);
                this.inverterParameterReceivedToken?.Dispose();
                this.inverterParameterReceivedToken = null;
            }
        }

        public string Filename(IEnumerable<Inverter> source, DriveInfo drive, bool unique)
        {
            var model = this.sessionService.MachineIdentity;
            var serial = model.SerialNumber;
            if (string.IsNullOrEmpty(nameof(serial)))
            {
                throw new ArgumentException("Cannot retrieve a serial code from the configuration.", nameof(source));
            }
            var name = Regex.Replace(serial, @"[^\w\.-]", string.Empty);
            string tick = default, filename = default;
            var incremental = 0;

            do
            {
                filename = System.IO.Path.Combine(
                  (drive ?? throw new ArgumentNullException(nameof(drive))).RootDirectory.FullName,
                  string.Format(System.Globalization.CultureInfo.InvariantCulture, "vertimag-Inverter-configuration.{0}-{1}{2}.json", name, AssemblyInfo.Version, tick));

                incremental++;
                tick = string.Concat("(", incremental, ")");
            } while (unique && File.Exists(filename));

            return filename;
        }

        public string Filename(VertimagConfiguration source, DriveInfo drive, bool unique)
        {
            var model = this.sessionService.MachineIdentity;
            var serial = model.SerialNumber;
            if (string.IsNullOrEmpty(nameof(serial)))
            {
                throw new ArgumentException("Cannot retrieve a serial code from the configuration.", nameof(source));
            }
            var name = Regex.Replace(serial, @"[^\w\.-]", string.Empty);
            string tick = default, filename = default;
            var incremental = 0;

            do
            {
                filename = System.IO.Path.Combine(
                  (drive ?? throw new ArgumentNullException(nameof(drive))).RootDirectory.FullName,
                  string.Format(System.Globalization.CultureInfo.InvariantCulture, "vertimag-configuration.{0}-{1}{2}.json", name, AssemblyInfo.Version, tick));

                incremental++;
                tick = string.Concat("(", incremental, ")");
            } while (unique && File.Exists(filename));

            return filename;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.SubscribeEvents();
        }

        private void OnInverterParameterReceived(NotificationMessageUI<InverterParametersMessageData> message)
        {
            if (message.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd)//read/write parameter
            {
                this.ShowNotification(message.Data.ToString(), Services.Models.NotificationSeverity.Info);
            }
            else if (message.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationUpdateData)//import structure command
            {
                this.ShowNotification(message.Data.Value, Services.Models.NotificationSeverity.Info);
            }
        }

        private void OnInverterProgrammingMessageReceived(NotificationMessageUI<InverterProgrammingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    this.IsBusy = true;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingStarted"), Services.Models.NotificationSeverity.Info);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingSuccessfullyEnded"), Services.Models.NotificationSeverity.Success);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingEndedErrors"), Services.Models.NotificationSeverity.Error);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InvertersProgrammingStopped"), Services.Models.NotificationSeverity.Warning);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd:
                    this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingNext"), Services.Models.NotificationSeverity.Info);
                    break;

                default:
                    break;
            }
        }

        private void OnInverterReadingMessageReceived(NotificationMessageUI<InverterReadingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingSuccessfullyEnded"), Services.Models.NotificationSeverity.Success);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingEndedErrors"), Services.Models.NotificationSeverity.Error);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    this.IsBusy = true;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingStarted"), Services.Models.NotificationSeverity.Info);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InvertersReadingStopped"), Services.Models.NotificationSeverity.Warning);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd:
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingNext"), Services.Models.NotificationSeverity.Info);
                    break;

                default:
                    break;
            }
        }

        private void SubscribeEvents()
        {
            this.inverterProgrammingMessageReceivedToken = this.inverterProgrammingMessageReceivedToken
              ?? this.EventAggregator
                  .GetEvent<NotificationEventUI<InverterProgrammingMessageData>>()
                  .Subscribe(
                      (m) => this.OnInverterProgrammingMessageReceived(m),
                      ThreadOption.UIThread,
                      false);

            this.inverterReadingMessageReceivedToken = this.inverterReadingMessageReceivedToken
               ?? this.EventAggregator
                   .GetEvent<NotificationEventUI<InverterReadingMessageData>>()
                   .Subscribe(
                       (m) => this.OnInverterReadingMessageReceived(m),
                       ThreadOption.UIThread,
                       false);

            this.inverterParameterReceivedToken = this.inverterParameterReceivedToken
               ?? this.EventAggregator
                   .GetEvent<NotificationEventUI<InverterParametersMessageData>>()
                   .Subscribe(
                       (m) => this.OnInverterParameterReceived(m),
                       ThreadOption.UIThread,
                       false);
        }

        #endregion
    }
}
