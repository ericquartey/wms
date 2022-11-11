using System;
using System.Collections.ObjectModel;
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

        private ObservableCollection<PresentationNotificationMessage> listPtlMessage = new ObservableCollection<PresentationNotificationMessage>();

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

        public ObservableCollection<PresentationNotificationMessage> ListPtlMessage
        {
            get => this.listPtlMessage;
            set => this.SetProperty(ref this.listPtlMessage, value);
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
            this.ListPtlMessage.Clear();
            this.BarcodeString = string.Empty;
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
                    this.ListPtlMessage.Clear();
                    this.ListPtlMessage.Add(message);
                    break;
                case NotificationSeverity.PtlInfo:
                    this.ListPtlMessage.Add(message);
                    break;
                case NotificationSeverity.PtlWarning:
                case NotificationSeverity.PtlError:
                    this.PtlErrorWarning = message;
                    break;
                case NotificationSeverity.PtlSuccess:
                    this.NavigationService.GoBack();
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
                    if (message.NotificationSeverity == NotificationSeverity.PtlInfoStart)
                    {
                        this.ListPtlMessage.Add(message);
                    }
                    else
                    {
                        this.PtlErrorWarning = message;
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
