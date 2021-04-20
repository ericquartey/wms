using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchDetailViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IBayManager bayManager;

        private readonly Services.IDialogService dialogService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly IWmsDataProvider wmsDataProvider;

        private int? inputQuantity;

        private bool isBusyRequestingItemPick;

        private bool isBusyRequestingItemPut;

        private ItemInfo item;

        private string itemTxt;

        private DelegateCommand requestItemPickCommandDetail;

        private DelegateCommand requestItemPutCommandDetail;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(
            Services.IDialogService dialogService,
            IWmsDataProvider wmsDataProvider,
            IMachineItemsWebService itemsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IBayManager bayManager,
            IAuthenticationService authenticationService)
            : base(PresentationMode.Operator)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public int? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyRequestingItemPick
        {
            get => this.isBusyRequestingItemPick;
            private set => this.SetProperty(ref this.isBusyRequestingItemPick, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyRequestingItemPut
        {
            get => this.isBusyRequestingItemPut;
            private set => this.SetProperty(ref this.isBusyRequestingItemPut, value, this.RaiseCanExecuteChanged);
        }

        public ItemInfo Item
        {
            get => this.item;
            set
            {
                if (value is null)
                {
                    this.RaisePropertyChanged();
                    return;
                }

                this.SetProperty(ref this.item, value);
                this.ItemTxt = string.Format(Resources.Localized.Get("OperatorApp.RequestedQuantity"), this.item.MeasureUnit);
                this.RaisePropertyChanged(nameof(this.ItemTxt));
            }
        }

        public string ItemTxt
        {
            get => this.itemTxt;
            set => this.SetProperty(ref this.itemTxt, value);
        }

        public ICommand RequestItemPickCommandDetail =>
            this.requestItemPickCommandDetail
            ??
            (this.requestItemPickCommandDetail = new DelegateCommand(
                async () => await this.RequestItemPickAsync(),
                this.CanRequestItemPick));

        public ICommand RequestItemPutCommandDetail =>
           this.requestItemPutCommandDetail
           ??
           (this.requestItemPutCommandDetail = new DelegateCommand(
               async () => await this.RequestItemPutAsync(),
               this.CanRequestItemPut));

        #endregion

        #region Methods

        public async Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (e.UserAction is UserAction.FilterItems)
            {
                await this.FilterItemsByCodeAsync(e);
            }
        }

        public override void Disappear()
        {
            this.Item = null;

            base.Disappear();
        }

        public async Task ExecuteItemPickAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPick = true;

                await this.wmsDataProvider.PickAsync(
                    this.Item.Id,
                    this.InputQuantity.Value,
                    reasonId: null,
                    reasonNotes: null,
                    lot: this.Item.Lot,
                    serialNumber: this.Item.SerialNumber,
                    userName: this.authenticationService.UserName);

                this.ShowNotification(
                    string.Format(
                        Localized.Get("OperatorApp.PickRequestWasAccepted"),
                        this.Item.Code,
                        this.InputQuantity),
                    Services.Models.NotificationSeverity.Success);

                this.logger.Debug($"Item pick");
                this.NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.InputQuantity = 0;
                this.IsBusyRequestingItemPick = false;
                this.IsWaitingForResponse = false;
            }
        }

        public async Task ExecuteItemPutAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPut = true;

                await this.wmsDataProvider.PutAsync(
                    this.item.Id,
                    this.InputQuantity.Value,
                    reasonId: null,
                    reasonNotes: null,
                    lot: this.Item.Lot,
                    serialNumber: this.Item.SerialNumber,
                    userName: this.authenticationService.UserName);

                this.ShowNotification(
                   string.Format(
                       Localized.Get("OperatorApp.PutRequestWasAccepted"),
                       this.item.Id,
                       this.InputQuantity),
                   Services.Models.NotificationSeverity.Success);

                this.logger.Debug($"Item put");
                this.NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.InputQuantity = 0;
                this.IsBusyRequestingItemPut = false;
                this.IsWaitingForResponse = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as ItemInfo;

            this.ItemTxt = string.Format(Localized.Get("OperatorApp.RequestedQuantity"), this.Item.MeasureUnit);

            this.RaisePropertyChanged(nameof(this.ItemTxt));

            this.InputQuantity = 0;
        }

        public async Task RequestItemPickAsync()
        {
            var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), string.Concat(Localized.Get("OperatorApp.PickArticle"), this.Item.Code), DialogType.Question, DialogButtons.YesNo);

            if (messageBoxResult is DialogResult.Yes)
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPick = true;

                //var waitForReason = await this.CheckReasonsAsync();

                //if (!waitForReason)
                //{
                //    await this.ExecuteItemPickAsync();
                //

                await this.ExecuteItemPickAsync();
            }
        }

        public async Task RequestItemPutAsync()
        {
            var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), string.Concat(Localized.Get("OperatorApp.PutArticle"), this.Item.Code), DialogType.Question, DialogButtons.YesNo);

            if (messageBoxResult is DialogResult.Yes)
            {
                this.IsWaitingForResponse = true;
                this.IsBusyRequestingItemPut = true;

                //var waitForReason = await this.CheckReasonsAsync();

                //if (!waitForReason)
                //{
                //    await this.ExecuteItemPutAsync();
                //}

                await this.ExecuteItemPutAsync();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.requestItemPickCommandDetail?.RaiseCanExecuteChanged();
            this.requestItemPutCommandDetail?.RaiseCanExecuteChanged();
        }

        private bool CanRequestItemPick()
        {
            return
                this.Item != null
                &&
                this.Item.AvailableQuantity.HasValue
                &&
                this.Item.AvailableQuantity.Value > 0
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity > 0
                &&
                this.InputQuantity <= this.Item.AvailableQuantity.Value
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanRequestItemPut()
        {
            return
                this.Item != null
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity > 0
                &&
                !this.IsWaitingForResponse;
        }

        private async Task FilterItemsByCodeAsync(UserActionEventArgs e)
        {
            var itemCode = e.GetItemCode();
            if (itemCode != null)
            {
                try
                {
                    var item = await this.itemsWebService.GetByBarcodeAsync(itemCode);
                    this.Item = new ItemInfo(item, this.bayManager.Identity.Id);
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
            }
        }

        #endregion
    }
}
