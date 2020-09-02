using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchDetailViewModel : BaseOperatorViewModel, IOperationalContextViewModel, IOperationReasonsSelector
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly IWmsDataProvider wmsDataProvider;

        private DelegateCommand cancelReasonCommand;

        private DelegateCommand confirmReasonCommand;

        private int? inputQuantity;

        private bool isBusyRequestingItemPick;

        private ItemInfo item;

        private string itemTxt;

        private int? reasonId;

        private string reasonNotes;

        private IEnumerable<OperationReason> reasons;

        private DelegateCommand requestItemPickCommandDetail;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(
            IWmsDataProvider wmsDataProvider,
            IMachineItemsWebService itemsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemsSearch.ToString();

        public ICommand CancelReasonCommand =>
            this.cancelReasonCommand
            ??
            (this.cancelReasonCommand = new DelegateCommand(
                this.CancelReason));

        public ICommand ConfirmReasonCommand =>
          this.confirmReasonCommand
          ??
          (this.confirmReasonCommand = new DelegateCommand(
              async () => await this.ExecuteItemPickAsync(),
              this.CanExecuteItemPick));

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

        public int? ReasonId
        {
            get => this.reasonId;
            set => this.SetProperty(ref this.reasonId, value, this.RaiseCanExecuteChanged);
        }

        public string ReasonNotes
        {
            get => this.reasonNotes;
            set => this.SetProperty(ref this.reasonNotes, value);
        }

        public IEnumerable<OperationReason> Reasons
        {
            get => this.reasons;
            set => this.SetProperty(ref this.reasons, value);
        }

        public ICommand RequestItemPickCommandDetail =>
                                    this.requestItemPickCommandDetail
            ??
            (this.requestItemPickCommandDetail = new DelegateCommand(
                async () => await this.RequestItemPickAsync(),
                this.CanRequestItemPick));

        #endregion

        #region Methods

        public async Task<bool> CheckReasonsAsync()
        {
            this.ReasonId = null;

            try
            {
                this.IsWaitingForResponse = true;
                this.ReasonNotes = null;

                this.Reasons = await this.missionOperationsWebService.GetAllReasonsAsync(MissionOperationType.Pick);

                if (this.reasons?.Any() == true)
                {
                    if (this.reasons.Count() == 1)
                    {
                        this.ReasonId = this.reasons.First().Id;
                    }
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.Reasons = null;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            return this.Reasons?.Any() == true;
        }

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
                    this.InputQuantity.Value);

                this.ShowNotification(
                    string.Format(
                        Resources.Localized.Get("OperatorApp.PickRequestWasAccepted"),
                        this.Item.Code,
                        this.InputQuantity),
                    Services.Models.NotificationSeverity.Success);

                this.Reasons = null;

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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.Reasons = null;

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as ItemInfo;

            this.ItemTxt = string.Format(Resources.Localized.Get("OperatorApp.RequestedQuantity"), this.Item.MeasureUnit);

            this.RaisePropertyChanged(nameof(this.ItemTxt));

            this.InputQuantity = 0;
        }

        public async Task RequestItemPickAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsBusyRequestingItemPick = true;

            var waitForReason = await this.CheckReasonsAsync();

            if (!waitForReason)
            {
                await this.ExecuteItemPickAsync();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.requestItemPickCommandDetail?.RaiseCanExecuteChanged();
            this.cancelReasonCommand?.RaiseCanExecuteChanged();
            this.confirmReasonCommand?.RaiseCanExecuteChanged();
        }

        private void CancelReason()
        {
            this.Reasons = null;
        }

        private bool CanExecuteItemPick()
        {
            return !(this.reasonId is null);
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
