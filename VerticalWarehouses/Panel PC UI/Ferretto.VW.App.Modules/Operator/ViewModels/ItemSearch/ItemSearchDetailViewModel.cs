using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchDetailViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineItemsWebService itemsWebService;

        private Item item;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(
            IMachineItemsWebService itemsWebService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemsSearch.ToString();

        public override EnableMask EnableMask => EnableMask.Any;

        public Item Item
        {
            get => this.item;
            set => this.SetProperty(ref this.item, value);
        }

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
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as Item;
        }

        private async Task ShowItemDetailsByBarcodeAsync(UserActionEventArgs e)
        {
            var itemBarcode = e.GetItemId();
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
