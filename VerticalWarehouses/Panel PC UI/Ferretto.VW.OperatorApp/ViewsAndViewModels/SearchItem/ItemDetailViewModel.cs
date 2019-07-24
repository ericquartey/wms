using System.Drawing;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.SearchItem
{
    public class ItemDetailViewModel : BaseViewModel, IItemDetailViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IWmsImagesProvider wmsImagesProvider;

        private DataGridItem article;

        private string articleCode;

        private string articleDescription;

        private Image image;

        #endregion

        #region Constructors

        public ItemDetailViewModel(
            IEventAggregator eventAggregator,
            IWmsImagesProvider wmsImagesProvider)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (wmsImagesProvider == null)
            {
                throw new System.ArgumentNullException(nameof(wmsImagesProvider));
            }

            this.eventAggregator = eventAggregator;
            this.wmsImagesProvider = wmsImagesProvider;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public DataGridItem Article
        {
            get => this.article;
            set
            {
                this.article = value;
                this.articleCode = this.article.Article;
                this.articleDescription = this.article.Description;
            }
        }

        public string ArticleCode { get => this.articleCode; set => this.SetProperty(ref this.articleCode, value); }

        public string ArticleDescription { get => this.articleDescription; set => this.SetProperty(ref this.articleDescription, value); }

        public Image Image { get => this.image; set => this.SetProperty(ref this.image, value); }

        #endregion

        #region Methods

        public override void ExitFromViewMethod()
        {
            this.image?.Dispose();
            this.Image?.Dispose();
        }

        public override async Task OnEnterViewAsync()
        {
            this.image?.Dispose();
            this.Image?.Dispose();
            this.image = null;
            this.Image = null;
            var stream = await this.wmsImagesProvider.GetImageAsync(this.Article.ImageCode);
            this.Image = Image.FromStream(stream);
        }

        #endregion
    }
}
