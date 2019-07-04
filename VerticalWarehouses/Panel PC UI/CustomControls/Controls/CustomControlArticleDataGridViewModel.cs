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

        #region Properties

        public ObservableCollection<TestArticle> Articles { get => this.articles; set => this.SetProperty(ref this.articles, value); }

        public BindableBase NavigationViewModel { get; set; }

        public TestArticle SelectedArticle { get => this.selectedArticle; set => this.SetProperty(ref this.selectedArticle, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            // TODO
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
