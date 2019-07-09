using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlArticleDataGridViewModel : BindableBase, ICustomControlArticleDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridItem> articles;

        private DataGridItem selectedArticle;

        #endregion

        #region Properties

        public ObservableCollection<DataGridItem> Articles { get => this.articles; set => this.SetProperty(ref this.articles, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridItem SelectedArticle { get => this.selectedArticle; set => this.SetProperty(ref this.selectedArticle, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            // HACK
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
