using System;
using System.Drawing;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemSearchDetailViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IWmsImagesProvider wmsImagesProvider;

        private Image image;

        private Item item;

        #endregion

        #region Constructors

        public ItemSearchDetailViewModel(IWmsImagesProvider wmsImagesProvider)
            : base(PresentationMode.Operator)
        {
            this.wmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public Image Image
        {
            get => this.image;
            set => this.SetProperty(ref this.image, value);
        }

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

            this.Image?.Dispose();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as Item;

            this.LoadItemImage();
        }

        private async Task LoadItemImage()
        {
            this.Image?.Dispose();
            this.Image = null;

            if (this.Item == null)
            {
                return;
            }

            var stream = await this.wmsImagesProvider.GetImageAsync(this.Item.Id.ToString());
            this.Image = Image.FromStream(stream);
        }

        #endregion
    }
}
