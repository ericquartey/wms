using System;
using System.Drawing;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemSearchDetailViewModel : BaseMainViewModel
    {
        #region Fields

        public readonly IItemSearchedModel itemSearchedModel;

        private readonly IWmsImagesProvider wmsImagesProvider;

        private Image image;

        private Item item;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(IWmsImagesProvider wmsImagesProvider, IItemSearchedModel itemSearchedModel)
            : base(PresentationMode.Operator)
        {
            this.wmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
            this.itemSearchedModel = itemSearchedModel ?? throw new ArgumentNullException(nameof(itemSearchedModel));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.None;

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        public Item Item
        {
            get => this.item;
            set => this.item = value;
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.Image?.Dispose();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            // Sistema di cache, potenzialmente errato!
            if (this.itemSearchedModel != null &&
                this.itemSearchedModel.SelectedItem != null)
            {
                this.Item = this.itemSearchedModel.SelectedItem;

                // await this.LoadImage(this.item.Code);
                this.RaisePropertyChanged(nameof(this.Item));
            }
            else
            {
                // effettuare la chiamata?
            }
        }

        private async Task LoadImage(string code)
        {
            this.Image?.Dispose();
            this.Image = null;
            var stream = await this.wmsImagesProvider.GetImageAsync(code);
            this.Image = Image.FromStream(stream);
        }

        #endregion
    }
}
