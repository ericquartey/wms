using System.Collections.ObjectModel;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlArticleDataGridViewModel : BaseViewModel, ICustomControlArticleDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridItem> articles;

        private DataGridItem selectedArticle;

        #endregion

        #region Properties

        public ObservableCollection<DataGridItem> Articles { get => this.articles; set => this.SetProperty(ref this.articles, value); }

        public DataGridItem SelectedArticle { get => this.selectedArticle; set => this.SetProperty(ref this.selectedArticle, value); }

        #endregion
    }
}
