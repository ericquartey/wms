using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CustomControls;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Net;
using System.IO;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Ferretto.VW.WmsCommunication.Interfaces;
using System.Windows.Media;
using System.Drawing;
using Ferretto.VW.CustomControls.Utils;

    public class ItemDetailViewModel : BindableBase, IItemDetailViewModel
    {
        #region Fields

        private DataGridItem article;

        private string articleCode;

        private string articleDescription;

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private Image image;

        private IWmsImagesProvider wmsImagesProvider;

        #endregion

        #region Constructors

        public ItemDetailViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
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

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.image?.Dispose();
            this.Image?.Dispose();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.wmsImagesProvider = this.container.Resolve<IWmsImagesProvider>();
        }

        public async Task OnEnterViewAsync()
        {
            this.image?.Dispose();
            this.Image?.Dispose();
            this.image = null;
            this.Image = null;
            var stream = await this.wmsImagesProvider.GetImageAsync(this.Article.ImageCode);
            this.Image = Image.FromStream(stream);
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
