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

        private readonly IPutToLightBarcodeService putToLightBarcodeService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private SubscriptionToken presentationNotificationToken;

        private bool isBusy;

        private PresentationNotificationMessage ptlMessage;

        private PresentationNotificationMessage ptlMessage2;

        private ObservableCollection<PresentationNotificationMessage> listPtlMessage = new ObservableCollection<PresentationNotificationMessage>();

        private bool hasErrors;




        private DelegateCommand showBarcodeReaderCommand;

        private DelegateCommand barcodeReaderCancelCommand;

        private DelegateCommand barcodeReaderConfirmCommand;

        private bool isVisibleBarcodeReader;

        private string barcodeString = "1";

        #endregion

        #region Constructors

        public PtlMessageViewModel(IPutToLightBarcodeService putToLightBarcodeService,
            IBarcodeReaderService barcodeReaderService)
                : base(PresentationMode.Operator)
        {
            this.putToLightBarcodeService = putToLightBarcodeService ?? throw new ArgumentNullException(nameof(putToLightBarcodeService));
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
        }

        #endregion

        #region Properties

        public PresentationNotificationMessage PtlMessage
        {
            get => this.ptlMessage;
            set => this.SetProperty(ref this.ptlMessage, value);
        }

        public PresentationNotificationMessage PtlMessage2
        {
            get => this.ptlMessage2;
            set => this.SetProperty(ref this.ptlMessage2, value);
        }

        public ObservableCollection<PresentationNotificationMessage> ListPtlMessage
        {
            get => this.listPtlMessage;
            set => this.SetProperty(ref this.listPtlMessage, value);
        }

        public bool HasErrors
        {
            get => this.hasErrors;
            set => this.SetProperty(ref this.hasErrors, value);
        }

        public ICommand ShowBarcodeReaderCommand =>
           this.showBarcodeReaderCommand
           ??
           (this.showBarcodeReaderCommand = new DelegateCommand(this.ShowBarcodeReader));

        public ICommand BarcodeReaderCancelCommand =>
                                    this.barcodeReaderCancelCommand
                    ??
                    (this.barcodeReaderCancelCommand = new DelegateCommand(
                        async () => this.BarcodeReaderCancel()));

        public ICommand BarcodeReaderConfirmCommand =>
                                            this.barcodeReaderConfirmCommand
                    ??
                    (this.barcodeReaderConfirmCommand = new DelegateCommand(
                        async () => this.BarcodeReaderConfirm()));
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
            this.PtlMessage2 = new PresentationNotificationMessage("", NotificationSeverity.NotSpecified);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        public void NotificationChanged(PresentationNotificationMessage message)
        {
            if (message is null)
            {
                return;
            }

            switch (message.NotificationSeverity)
            {
                case NotificationSeverity.PtlInfo:
                    this.ListPtlMessage.Add(message);
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
                this.isBusy = true;

                if (this.Data is PresentationNotificationMessage message)
                {
                    this.PtlMessage = message;
                    this.ListPtlMessage.Add(message);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.isBusy = false;
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
            this.IsVisibleBarcodeReader = true;
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
