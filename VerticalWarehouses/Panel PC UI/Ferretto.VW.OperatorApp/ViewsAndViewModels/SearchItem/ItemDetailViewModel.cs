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

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem
{
    public class ItemDetailViewModel : BindableBase, IItemDetailViewModel
    {
        #region Fields

        private TestArticle article;

        private string articleCode;

        private string articleDescription;

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private string imagePath;

        #endregion

        #region Constructors

        public ItemDetailViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public TestArticle Article
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

        public string ImagePath { get => this.imagePath; set => this.SetProperty(ref this.imagePath, value); }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.ImagePath = "./Images/image.jpg";
        }

        public async Task OnEnterViewAsync()
        {
            //var webClient = new WebClient();
            //webClient.DownloadFile("http://172.16.199.100:6000/api/images/Articolo1.jpg", "./Images/image.jpg");
            //webClient.Dispose();

            //var imagesProvider = this.container.Resolve<IWmsImagesProvider>();
            //var image = imagesProvider.GetImageAsync(this.Article.Article);
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
