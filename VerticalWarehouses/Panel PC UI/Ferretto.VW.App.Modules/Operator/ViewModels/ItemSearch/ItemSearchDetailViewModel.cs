using System;
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
    public class ItemSearchDetailViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IWmsDataProvider wmsDataProvider;

        private double? inputQuantity;

        private bool isBusyRequestingItemPick;

        private ItemInfo item;

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

        public ICommand RequestItemPickCommand =>
            this.requestItemPickCommand
            ??
            (this.requestItemPickCommand = new DelegateCommand(
                async () => await this.RequestItemPickAsync(),
                this.CanRequestItemPick));

        #endregion

        #region Methods

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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as ItemInfo;

            this.InputQuantity = null;
        }

        public async Task RequestItemPickAsync()
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

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.requestItemPickCommand?.RaiseCanExecuteChanged();
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
