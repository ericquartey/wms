using System;
using System.Drawing;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchDetailViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IItemsWmsWebService itemsWmsWebService;

        private readonly IWmsImagesProvider wmsImagesProvider;

        private Item item;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IItemsWmsWebService itemsWmsWebService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.wmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.itemsWmsWebService = itemsWmsWebService ?? throw new ArgumentNullException(nameof(itemsWmsWebService));
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

        protected override async Task OnBarcodeMatchedAsync(BarcodeMatchEventArgs e)
        {
            await base.OnBarcodeMatchedAsync(e);

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

        private async Task ShowItemDetailsByBarcodeAsync(BarcodeMatchEventArgs e)
        {
            var itemBarcode = e.GetItemBarCode();
            if (itemBarcode != null)
            {
                try
                {
                    var item = await this.itemsWmsWebService.GetByBarcodeAsync(itemBarcode);
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
