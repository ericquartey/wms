using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class PtlMessageViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private SubscriptionToken presentationNotificationToken;

        private PresentationNotificationMessage ptlStartInfo;

        private PresentationNotificationMessage ptlInfo1 = null;

        private PresentationNotificationMessage ptlInfo2 = null;

        private PresentationNotificationMessage ptlErrorWarning = null;

        private DelegateCommand showBarcodeReaderCommand;

        private DelegateCommand barcodeReaderCancelCommand;

        private DelegateCommand barcodeReaderConfirmCommand;

        private bool isVisibleBarcodeReader;

        private string barcodeString;

        #endregion

        #region Constructors

        public PtlMessageViewModel(IPutToLightBarcodeService putToLightBarcodeService,
            IBarcodeReaderService barcodeReaderService)
                : base(PresentationMode.Operator)
        {
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
        }

        #endregion

        #region Properties

        public PresentationNotificationMessage PtlErrorWarning
        {
            get => this.ptlErrorWarning;
            set => this.SetProperty(ref this.ptlErrorWarning, value);
        }

        public PresentationNotificationMessage PtlStartInfo
        {
            get => this.ptlStartInfo;
            set => this.SetProperty(ref this.ptlStartInfo, value);
        }

        public PresentationNotificationMessage PtlInfo1
        {
            get => this.ptlInfo1;
            set => this.SetProperty(ref this.ptlInfo1, value);
        }

        public PresentationNotificationMessage PtlInfo2
        {
            get => this.ptlInfo2;
            set => this.SetProperty(ref this.ptlInfo2, value);
        }

        public ICommand ShowBarcodeReaderCommand =>
           this.showBarcodeReaderCommand
           ??
           (this.showBarcodeReaderCommand = new DelegateCommand(this.ShowBarcodeReader));

        public ICommand BarcodeReaderCancelCommand =>
            this.barcodeReaderCancelCommand
            ??
            (this.barcodeReaderCancelCommand = new DelegateCommand(async () => this.BarcodeReaderCancel()));

        public ICommand BarcodeReaderConfirmCommand =>
            this.barcodeReaderConfirmCommand
            ??
            (this.barcodeReaderConfirmCommand = new DelegateCommand(async () => this.BarcodeReaderConfirm(), this.CanConfirm));

        public bool IsVisibleBarcodeReader
        {
            get => this.isVisibleBarcodeReader;
            set => this.SetProperty(ref this.isVisibleBarcodeReader, value, this.RaiseCanExecuteChanged);
        }

        public string BarcodeString
        {
            get => this.barcodeString;
            set => this.SetProperty(ref this.barcodeString, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        private bool CanConfirm()
        {
            return !string.IsNullOrEmpty(this.BarcodeString);
        }

        public override async Task OnAppearedAsync()
        {
            this.Subscribe();

            await this.LoadData();

            await base.OnAppearedAsync();
        }

        public override void Disappear()
        {
            base.Disappear();

            this.UnSubscribe();

            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

            this.PtlStartInfo = null;
            this.PtlInfo1 = null;
            this.PtlInfo2 = null;
            this.PtlErrorWarning = null;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.barcodeReaderConfirmCommand.RaiseCanExecuteChanged();
        }

        public void NotificationChanged(PresentationNotificationMessage message)
        {
            if (message is null)
            {
                return;
            }

            switch (message.NotificationSeverity)
            {
                case NotificationSeverity.PtlInfoStart:
                    this.PtlStartInfo = message;
                    this.PtlInfo1 = null;
                    this.PtlInfo2 = null;
                    this.PtlErrorWarning = null;
                    break;
                case NotificationSeverity.PtlInfo1:
                    this.PtlInfo1 = message;
                    this.PtlErrorWarning = null;
                    break;
                case NotificationSeverity.PtlInfo2:
                    this.PtlInfo2 = message;
                    this.PtlErrorWarning = null;
                    break;
                case NotificationSeverity.PtlSuccess:
                    this.NavigationService.GoBack();
                    this.PtlErrorWarning = null;
                    break;
                case NotificationSeverity.PtlWarning:
                case NotificationSeverity.PtlError:
                    this.PtlErrorWarning = message;
                    break;
                default:
                    break;
            }
        }

        private async Task LoadData()
        {
            try
            {
                if (this.Data is PresentationNotificationMessage message)
                {
                    switch (message.NotificationSeverity)
                    {
                        case NotificationSeverity.PtlInfoStart:
                            this.PtlStartInfo = message;
                            break;
                        case NotificationSeverity.PtlInfo1:
                            this.PtlInfo1 = message;
                            break;
                        case NotificationSeverity.PtlInfo2:
                            this.PtlInfo2 = message;
                            break;
                        case NotificationSeverity.PtlWarning:
                        case NotificationSeverity.PtlError:
                            this.PtlErrorWarning = message;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void Subscribe()
        {
            this.presentationNotificationToken = this.presentationNotificationToken
                ??
                this.EventAggregator.GetEvent<PresentationNotificationPubSubEvent>()
                                    .Subscribe(notificationMessage => this.NotificationChanged(notificationMessage), ThreadOption.UIThread, true);
        }

        private void UnSubscribe()
        {
            this.EventAggregator.GetEvent<PresentationNotificationPubSubEvent>()
                                .Unsubscribe(this.presentationNotificationToken);

            this.presentationNotificationToken = null;
        }

        protected void ShowBarcodeReader()
        {
            this.IsVisibleBarcodeReader = !this.IsVisibleBarcodeReader;
        }

        public void BarcodeReaderCancel()
        {
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;
        }

        public void BarcodeReaderConfirm()
        {
            if (!string.IsNullOrEmpty(this.BarcodeString))
            {
                this.barcodeReaderService.SimulateRead(this.BarcodeString.EndsWith("\r") ? this.BarcodeString : this.BarcodeString + "\r");

                this.BarcodeString = string.Empty;
                this.IsVisibleBarcodeReader = false;
            }
        }

        #endregion
    }
}
