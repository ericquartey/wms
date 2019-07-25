using System.Drawing;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.SearchItem
{
    public class ItemDetailViewModel : BaseViewModel, IItemDetailViewModel
    {
        #region Fields

        private readonly IWmsImagesProvider wmsImagesProvider;

        private Image image;

        private Item item;

        #endregion

        #region Constructors

        public ItemDetailViewModel(
            IWmsImagesProvider wmsImagesProvider)
        {
            if (wmsImagesProvider == null)
            {
                throw new System.ArgumentNullException(nameof(wmsImagesProvider));
            }

            this.wmsImagesProvider = wmsImagesProvider;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        public Item Item
        {
            get => this.item;
            set
            {
                this.item = value;
            }
        }

        #endregion

        #region Methods

        public override void ExitFromViewMethod()
        {
            this.Image?.Dispose();
        }

        public override async Task OnEnterViewAsync()
        {
            var searchViewModel = ServiceLocator.Current.GetInstance<IItemSearchViewModel>();

            if (searchViewModel != null &&
                searchViewModel.SelectedItem != null)
            {
                this.Item = searchViewModel.SelectedItem;
                await this.LoadImage(this.item.Code);
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
