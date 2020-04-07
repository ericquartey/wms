using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories;
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

        private readonly IWmsDataProvider wmsDataProvider;

        private DelegateCommand cancelReasonCommand;

        private DelegateCommand confirmReasonCommand;

        private double? inputQuantity;

        private bool isBusyRequestingItemPick;

        private ItemInfo item;

        private int? reasonId;

        private string reasonNotes;

        private IEnumerable<OperationReason> reasons;

        private DelegateCommand requestItemPickCommand;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(
            IWmsDataProvider wmsDataProvider,
            IMachineItemsWebService itemsWebService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
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

        public double? InputQuantity
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
            set => this.SetProperty(ref this.item, value);
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

        public ICommand RequestItemPickCommand =>
                                    this.requestItemPickCommand
            ??
            (this.requestItemPickCommand = new DelegateCommand(
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

                this.Reasons = null;
                //this.Reasons = await this.missionOperationsService.GetAllReasonsAsync(MissionOperationType.Pick);

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
                throw new ArgumentNullException(nameof(e));
            }

            if (Enum.TryParse<UserAction>(e.UserAction, out var userAction))
            {
                switch (userAction)
                {
                    case UserAction.FilterItems:
                        await this.ShowItemDetailsByBarcodeAsync(e);

                        break;

                    case UserAction.PickItem:

                        // TODO da definire con Danilo
                        break;
                }
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
                        Resources.OperatorApp.PickRequestWasAccepted,
                        this.Item.Code,
                        this.InputQuantity),
                    Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.InputQuantity = null;
                this.IsBusyRequestingItemPick = false;
                this.IsWaitingForResponse = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as ItemInfo;

            this.InputQuantity = null;
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

            this.requestItemPickCommand?.RaiseCanExecuteChanged();
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

        private async Task ShowItemDetailsByBarcodeAsync(UserActionEventArgs e)
        {
            var itemBarcode = e.GetItemCode();
            if (itemBarcode != null)
            {
                try
                {
                    var item = await this.itemsWebService.GetByBarcodeAsync(itemBarcode);
                    this.Item = new ItemInfo(item, this.bayManager.Identity.Id);
                }
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    this.ShowNotification(ex);
                }
            }
        }

        #endregion
    }
}
