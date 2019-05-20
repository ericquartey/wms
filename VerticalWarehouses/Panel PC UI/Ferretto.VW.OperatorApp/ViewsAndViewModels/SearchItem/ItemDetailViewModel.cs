using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CustomControls;
using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem
{
    public class ItemDetailViewModel : BindableBase, IItemDetailViewModel
    {
        #region Fields

        private TestArticle article;

        private string articleCode;

        private string articleDescription;

        private IEventAggregator eventAggregator;

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

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
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
