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

        private readonly IMachineIdentityWebService identityService;

        private SubscriptionToken inverterProgrammingMessageReceivedToken;

        private bool isBusy;

        #endregion

        #region Constructors

        public BaseParameterInverterViewModel(IMachineIdentityWebService identityService)
                : base(PresentationMode.Installer)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
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

        public bool CanSave()
        {
            return !this.isBusy;
        }

        public override void Disappear()
        {
            this.EventAggregator.GetEvent<NotificationEventUI<InverterProgrammingMessageData>>().Unsubscribe(this.inverterProgrammingMessageReceivedToken);
            this.inverterProgrammingMessageReceivedToken = null;

            base.Disappear();
        }

        public string Filename(IEnumerable<Inverter> source, DriveInfo drive, bool unique)
        {
            var model = this.identityService.GetAsync().Result;
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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.SubscribeEvents();
        }

        private void OnInverterProgrammingMessageReceived(NotificationMessageUI<InverterProgrammingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.isBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingSuccessfullyEnded"), Services.Models.NotificationSeverity.Success);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                    this.isBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingEndedErrors"), Services.Models.NotificationSeverity.Error);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    this.isBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InvertersProgrammingStopped"), Services.Models.NotificationSeverity.Warning);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd:
                    this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingNext"), Services.Models.NotificationSeverity.Warning);
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
        }

        #endregion
    }
}
