using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CustomControlArticleDataGridViewModel : BindableBase, ICustomControlArticleDataGridViewModel
    {
        #region Fields

        private ObservableCollection<TestArticle> articles;

        private TestArticle selectedArticle;

        #endregion

        #region Constructors

        public CustomControlArticleDataGridViewModel()
        {
            this.articles = new ObservableCollection<TestArticle>();

            for (int i = 0; i < 100; i++)
            {
                this.articles.Add(new TestArticle($"Article {i}", $"this is article {i}", $"{i},{i + 1}"));
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<TestArticle> Articles { get => this.articles; set => this.SetProperty(ref this.articles, value); }

        public BindableBase NavigationViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public TestArticle SelectedArticle { get => this.selectedArticle; set => this.SetProperty(ref this.selectedArticle, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            return null;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
